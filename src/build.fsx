// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open System.IO
open System.Linq

TraceEnvironmentVariables()
RestorePackages()
if buildServer = BuildServer.AppVeyor then 
    MSBuildLoggers <- @"""C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll""" :: MSBuildLoggers

// Version information
let majorVersion = 0
let minorVersion = 23
let patchLevel = 2
let buildVersion = System.DateTime.UtcNow.ToString("yyyyMMddhhmm")
let version = sprintf "%i.%i.%i-alpha+%s" majorVersion minorVersion patchLevel buildVersion
let productName = "FlexSearch"
let copyright = "(c) Seemant Rajvanshi, 2012 - 2014"
// Properties
let buildDir = @".\build\"
let testDir = @".\build\"
let deployDir = @".\deploy\"

// Create necessary directories if they don't exist
Directory.CreateDirectory(buildDir)
Directory.CreateDirectory(deployDir)

/// <summary>
/// Delete and move files to correct folders
/// </summary>
let packageFiles() = 
    let src = __SOURCE_DIRECTORY__ + @"\build"
    let dest = __SOURCE_DIRECTORY__ + @"\build\lib\"
    // Delete all pdb files
    for file in Directory.GetFiles(src, "*.pdb") do
        File.Delete(file)
    // Delete all xml files
    for file in Directory.GetFiles(src, "*.xml") do
        File.Delete(file)
    // Move all non flex related files to lib folder
    for file in Directory.GetFiles(src) do
        let fileName = Path.GetFileName(file).ToLowerInvariant()
        if fileName.Contains("test") then File.Delete(file)
        else 
            if fileName.StartsWith("flex") = false && fileName.StartsWith("install") = false 
               && fileName.StartsWith("uninstall") = false && fileName.StartsWith("license") = false 
               && fileName.StartsWith("benchmark") = false then File.Move(file, Path.Combine(dest, fileName))
    let filesToDelete = 
        ([ "xunit.dll"; "xunit.extensions.dll"; "visualize.dll"; "ploeh.autofixture.dll"; "ploeh.autofixture.xunit.dll"; "mono.cecil.dll"; "mono.cecil.mdb.dll"; "mono.cecil.pdb.dll"; "mono.cecil.rocks.dll" ])
            .ToList()
    // Delete unnecessary files from the lib
    for file in Directory.GetFiles(dest) do
        let fileName = Path.GetFileName(file)
        if fileName.EndsWith(".xml") then File.Delete(file)
        else 
            if filesToDelete.Contains(fileName) then File.Delete(file)
    let filesToMove = 
        ([ "FlexSearch.Connectors.dll"; "FlexSearch.Connectors.dll.config"; "FlexSearch.DuplicateDetection.dll"; 
           "FlexSearch.DuplicateDetection.dll.config" ])
    // Move plug-in to plug-in folder
    for file in Directory.GetFiles(src) do
        let fileName = Path.GetFileName(file)
        if filesToMove.Contains(fileName) then File.Move(file, Path.Combine(src, "Plugins", fileName))

let AssemblyInfo path title = 
    CreateFSharpAssemblyInfo (sprintf @".\%s\AssemblyInfo.fs" path) [ Attribute.Title title
                                                                      Attribute.Description title
                                                                      Attribute.Product productName
                                                                      Attribute.Copyright copyright
                                                                      Attribute.FileVersion version
                                                                      Attribute.Version version ]

let AssemblyInfoCSharp path title = 
    CreateCSharpAssemblyInfo (sprintf @".\%s\Properties\AssemblyInfo.cs" path) [ Attribute.Title title
                                                                                 Attribute.Description title
                                                                                 Attribute.Product productName
                                                                                 Attribute.Copyright copyright
                                                                                 Attribute.FileVersion version
                                                                                 Attribute.Version version ]

// Targets
Target "Clean" (fun _ -> CleanDirs [ buildDir; testDir; @"build\Conf"; @"build\Data"; @"build\Plugins"; @"build\Lib" ])
// This is to ensure that the compiled weaver is copied to the correct folder so that Fody can pick it up
Target "BuildWeaver" (fun _ -> 
    !!"weavers/weavers.fsproj"
    |> MSBuildRelease "weavers/bin/release" "Build"
    |> Log "BuildWeaver-Output: ")
Target "BuildApp" (fun _ -> 
    AssemblyInfo "FlexSearch.Server" "FlexSearch Server"
    AssemblyInfo "FlexSearch.Core" "FlexSearch Core Library"
    AssemblyInfo "FlexSearch.Api" "FlexSearch Api"
    AssemblyInfo "FlexSearch.Common" "FlexSearch Common Utilities"
    AssemblyInfo "FlexSearch.Connectors" "FlexSearch Coneectors"
    AssemblyInfo "FlexSearch.Client" "FlexSearch Client"
    AssemblyInfoCSharp "FlexSearch.Logging" "FlexSearch Logging Library"
    MSBuildRelease buildDir "Build" [ @"FlexSearch.sln" ] |> Log "BuildApp-Output: ")
Target "Test" (fun _ -> 
    let errorCode = 
        [ Path.Combine(testDir, "FlexSearch.Tests.exe") ]
        |> Seq.map (fun p -> asyncShellExec { defaultParams with Program = p })
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.sum
    if errorCode <> 0 then failwith "Error in tests")
Target "Default" (fun _ -> trace "FlexSearch Compilation")
Target "MoveFiles" (fun _ -> packageFiles())
Target "Zip" 
    (fun _ -> !!(buildDir + "/**/*.*") -- "*.zip" |> Zip buildDir (deployDir + "FlexSearch." + version + ".zip"))
// Dependencies
"Clean" ==> "BuildWeaver" ==> "BuildApp" // ==> "Test"
                                         ==> "Default" ==> "MoveFiles" ==> "Zip"
// start build
RunTargetOrDefault "Zip"

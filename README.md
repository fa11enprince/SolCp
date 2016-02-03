# SolCp
Copying Visual Studio solution file

This command-line tool copies a directory included solution file. 
It replaces project file path, replaces GUIDs.

## Usage
Open command prompt, then enter :

    > SolCp [solution src path] [solution dst path]

## Result
    [src]
    demo
    │  demo.sln
    └─demo
            demo.vcxproj
            demo.vcxproj.filters
            Source.cpp
    [dst]
    demoCopy
    │  demoCopy.sln
    └─demoCopy
            demoCopy.vcxproj
            demoCopy.vcxproj.filters
            Source.cpp

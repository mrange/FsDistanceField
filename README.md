# FsDistanceField
Computes a distance field PNG from an input image.

Distance fields are commonly used in shader world where it can be used to create decals and other graphics that dont' get blurry upon scaling and support things like drop shadow, inner glow, outer glow and so on "for free".

## Build & Run

```bash
cd src/FsDistanceField
# Print help screen
dotnet run -- -h
# Generate blazor distance field
dotnet run -c Release -- -i ../../assets/Blazor.png -o ../../assets/Blazor_distance.png -r 200 -c 0.5 -y true
```

# Licensed software

## FsDistanceField
BSD 2-Clause License

Copyright (c) 2022, Mårten Rånge

github : https://github.com/mrange/FsDistanceField
license: https://github.com/mrange/FsDistanceField/LICENSE

## Distance field algorithm based on tiny-sdf (Mapbox)
Copyright © 2016-2021 Mapbox, Inc.
This code available under the terms of the BSD 2-Clause license.

github : https://github.com/mapbox/tiny-sdf
license: https://github.com/mapbox/tiny-sdf/blob/main/LICENSE.txt

## Image processing library (SixLabors.ImageSharp)
Six Labors Split License
Version 1.0, June 2022
Copyright (c) Six Labors

Uses the Apache License, Version 2.0 branch of the Six Labors Split License as:
- FsDistanceField is fulfilling the requirement: You are consuming the Work in for use in software licensed under an Open Source or Source Available license.

github : https://github.com/SixLabors/ImageSharp
license: https://github.com/SixLabors/ImageSharp/blob/main/LICENSE

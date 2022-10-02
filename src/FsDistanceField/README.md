# FsDistanceField
Computes a distance field PNG from an input image.

Distance fields are commonly used in shader world where it can be used to create decals and other graphics that dont' get blurry upon scaling and support things like drop shadow, inner glow, outer glow and so on "for free".

## Install

```bash
# Install the tool globally
dotnet tool install -g FsDistanceField
# Print the help
create-distance-field --help
# Should respond with this
# Description:
#   Computes a distance field image from an input image
#
# Usage:
#   create-distance-field [options]
#
# Options:
#   --version                         Show version information
#   -?, -h, --help                    Show help and usage information
#   -i, --input <input> (REQUIRED)    Input image file to compute distance field for
#   -o, --output <output> (REQUIRED)  Output PNG file to for the distance field
#   -r, --radius <radius>             Radius of distance field in pixels [default: 32]
#   -c, --cutoff <cutoff>             Cutoff of distance field [default: 0.25]
#   -p, --padding <padding>           Padding applied to distance field image [default: 0]
#   -y, --overwrite                   Overwrite existing file [default: False]
```

## Licensed software

### FsDistanceField
BSD 2-Clause License

Copyright (c) 2022, Mårten Rånge

github : https://github.com/mrange/FsDistanceField
license: https://github.com/mrange/FsDistanceField/LICENSE

### Distance field algorithm based on tiny-sdf (Mapbox)
Copyright © 2016-2021 Mapbox, Inc.
This code available under the terms of the BSD 2-Clause license.

github : https://github.com/mapbox/tiny-sdf
license: https://github.com/mapbox/tiny-sdf/blob/main/LICENSE.txt

### Image processing library (SixLabors.ImageSharp)
Six Labors Split License
Version 1.0, June 2022
Copyright (c) Six Labors

Uses the Apache License, Version 2.0 branch of the Six Labors Split License as:
- FsDistanceField is fulfilling the requirement: You are consuming the Work in for use in software licensed under an Open Source or Source Available license.

github : https://github.com/SixLabors/ImageSharp
license: https://github.com/SixLabors/ImageSharp/blob/main/LICENSE

(*
Copyright 2022 Mårten Rånge

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*)

(*
The function 'createDistanceField' is based on mapbox's tiny-sdf implementation 
 here licensed under BSD 2-Clause license
 See mapbox license below
Mapbox tiny-sdf is based on the paper: 
 Distance Transforms of Sampled Functions
 https://cs.brown.edu/people/pfelzens/papers/dt-final.pdf

Copyright © 2016-2021 Mapbox, Inc.
This code available under the terms of the BSD 2-Clause license.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

github: https://github.com/mapbox/tiny-sdf
*)

(*
See SixLabors license if you need a commercial license:
  https://sixlabors.com/pricing/license/
*)

open System
open System.Globalization
open System.IO

open FSharp.Core.Printf

open FSharp.SystemCommandLine

open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing

module Log =
  let log cc (msg : string) =
    let occ = Console.ForegroundColor
    try
      Console.ForegroundColor <- cc
      Console.WriteLine msg
    finally
      Console.ForegroundColor <- occ

  let error     msg = log ConsoleColor.Red      msg
  let hilight   msg = log ConsoleColor.Cyan     msg
  let info      msg = log ConsoleColor.Gray     msg
  let success   msg = log ConsoleColor.Green    msg

  let errorf    fmt = kprintf error   fmt
  let hilightf  fmt = kprintf hilight fmt
  let infof     fmt = kprintf info    fmt
  let successf  fmt = kprintf success fmt
open Log

let inline clamp mi mx v = min (max v mi) mx

let createDistanceField 
  (radius  : int          )
  (cutoff  : float        )
  (buffer  : int          )
  (pwidth  : int          )
  (pheight : int          )
  (pixels  : Rgba32 array ) =

  let radius  = float radius

  let inf     = 1E20

  let width   = pwidth  + 2*buffer
  let height  = pheight + 2*buffer
  let size    = max width height
  let gridOuter : float   array = Array.create (width * height) inf
  let gridInner : float   array = Array.zeroCreate (width * height)
  let f         : float   array = Array.zeroCreate (size)
  let z         : float   array = Array.zeroCreate (size + 1)
  let v         : int     array = Array.zeroCreate (size)

  for y = 0 to pheight - 1 do
    for x = 0 to pwidth - 1 do
      let pixel = pixels.[x+y*pwidth]
      let a = pixel.A

      let off = x + buffer + (y + buffer)*width

      if a = 0uy then
        ()
      elif a = 255uy then
        gridOuter.[off] <- 0.
        gridInner.[off] <- inf
      else
        let a   = (float a)/255.
        let d   = 0.5 - a
        let d2  = d*d
        gridOuter.[off] <- if d > 0. then d2 else 0.
        gridInner.[off] <- if d < 0. then d2 else 0.

  // See: https://cs.brown.edu/people/pfelzens/papers/dt-final.pdf
#if DEBUG
  let edt1d (grid : float array) offset stride length =
#else
  let inline edt1d (grid : float array) offset stride length =
#endif
    let mutable k = 0

    v.[0] <- 0
    z.[0] <- -inf
    z.[1] <- inf
    f.[0] <- grid.[offset]

    for q = 1 to length - 1 do
      let fq = grid[offset + q * stride]
      f.[q] <- fq
      let q2 = float (q * q);
      let mutable cont = true
      let mutable s = 0.

      while cont do
        let vk = v.[k]
        s <- 0.5*((fq  - f.[vk]) + q2 - float (vk*vk))/(float (q - vk))
        cont <- s <= z.[k]
        if cont then
          k <- k - 1
          cont <- k > -1

      k <- k + 1

      v.[k] <- q
      z.[k] <- s
      z.[k + 1] <- inf

    k <- 0
    for q = 0 to length - 1 do
      while z.[k + 1] < q do
        k <- k + 1
      let vk = v.[k]
      let qr = float (q - vk)
      grid.[offset + q * stride] <- f[vk] + qr*qr 

#if DEBUG
  let edt grid x0 y0 w h =
#else
  let inline edt grid x0 y0 w h =
#endif
    for x = x0 to x0 + w - 1 do
      edt1d grid (y0 * width + x) width h
    for y = y0 to height - 1 do
      edt1d grid (y * width + x0) 1 w

  edt gridOuter 0      0      width            height
  edt gridInner buffer buffer (width - buffer) (height - buffer)

  let data      : byte array = Array.zeroCreate (width * height)

  for i = 0 to data.Length - 1 do
    let d = sqrt gridOuter.[i] - sqrt gridInner.[i]
    let d = round (255. - 255.*(d/radius+cutoff))
    let d = clamp 0. 255.0 d
    data.[i] <- byte d

  width, height, data

let computeDistanceField  ( inputFile   : FileInfo
                          , outputFile  : FileInfo
                          , radius      : int
                          , cutoff      : float
                          , padding     : int
                          , overwrite   : bool
                          ) =
  try
    let input   = inputFile.FullName
    let output  = outputFile.FullName

    let radius  = radius  |> clamp  2   512
    let cutoff  = cutoff  |> clamp -1.0 1.0
    let padding = padding |> clamp  0   512

    hilight "Computing distance field for" 
    hilightf "  input     : %s"   input
    hilightf "  output    : %s"   output
    hilightf "  radius    : %d"   radius
    hilightf "  cutoff    : %.2f" cutoff
    hilightf "  padding   : %d"   padding
    hilightf "  overwrite : %A"   overwrite

    if input = output                       then failwith "Input and output file can't be the same file"
    if not (File.Exists input)              then failwith "Input file doesn't exists"
    if not overwrite && File.Exists output  then failwith "Output file already exists, if you intend to overwrite pass the overwrite argument"

    let width, height, pixels =
      infof "Loading: %s" input
      use img   = Image.Load input
      infof "Image is %dx%d" img.Width img.Height

      info "Converting to RGBA32"
      use rgba = img.CloneAs<Rgba32> ()

      info "Extracting pixels"
      let pixels : Rgba32 array = Array.zeroCreate (rgba.Width*rgba.Height)
      rgba.CopyPixelDataTo pixels
      rgba.Width, rgba.Height, pixels

    info "Computing distance field"
    let width, height, distance = 
      createDistanceField 
        radius
        cutoff
        padding
        width 
        height 
        pixels

    let distance = distance |> Array.map L8

    use img = new Image<L8>(width, height)
    let b, memory = img.DangerousTryGetSinglePixelMemory ()
    if b then
      distance.AsSpan().CopyTo(memory.Span)
      infof "Saving distance field to: %s" output
      if not overwrite && File.Exists output  then failwith "Output file already exists, if you intend to overwrite pass the overwrite argument"
      img.SaveAsPng output
    else
      failwith "Failed to get pixel memory, aborting"

    success "All done!"

    0
  with
  | e ->
#if !DEBUG
    errorf "An error occurred: %s" e.Message
#else
    errorf "An error occurred: %s%s%s" e.Message Environment.NewLine (e.ToString ())
#endif
    99

[<EntryPoint>]
let main args =
  let ci = CultureInfo.InvariantCulture
  CultureInfo.CurrentCulture              <- ci
  CultureInfo.DefaultThreadCurrentCulture <- ci

  let inputFile   = Input.OptionRequired<FileInfo>  ([|"-i"; "--input"    |], "Input image file to compute distance field for"  )
  let outputFile  = Input.OptionRequired<FileInfo>  ([|"-o"; "--output"   |], "Output PNG file to for the distance field"       )

  let radius      = Input.Option<int>               ([|"-r"; "--radius"   |], 32    , "Radius of distance field in pixels"      )
  let cutoff      = Input.Option<float>             ([|"-c"; "--cutoff"   |], 0.25  , "Cutoff of distance field"                )
  let padding     = Input.Option<int>               ([|"-p"; "--padding"  |], 0     , "Padding applied to distance field image" )
  let overwrite   = Input.Option<bool>              ([|"-y"; "--overwrite"|], false , "Overwrite existing file"                 )

  rootCommand args {
    description "Computes a distance field image from an input image\nSee https://github.com/mrange/FsDistanceField#licensed-software to see licensed software used by this tool."
    inputs      (inputFile, outputFile, radius, cutoff, padding, overwrite)
    setHandler  computeDistanceField
  }

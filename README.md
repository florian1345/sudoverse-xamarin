# Sudoverse

This repository contains the Sudoverse app (see directory `app`). In addition,
it provides a native library (see directory `engine`) that is used by the `app`
to generate Sudoku. The engine is written in Rust and uses the
[`sudoku-variants`][sudoku-variants] crate internally.

# Building

Currently, only scripts for building on Windows are provided. You need to
fulfill the following prerequisites to build.

* Rustup and Cargo
* Microsoft VisualStudio with Xamarin.Forms and Visual C++ (specifically MSVC)
* Android NDK installed with an environment variable `NDK_HOME` pointing to its
root
* A version of `libgcc` provided in the directory `engine/lib` - this is
usually a file named `libgcc.a`

You can then use the following process to build the project.

* If it is your first time building, run `rustup-targets.bat` in the root
directory to setup all supported targets for Rustup.
* Run `prepare-engine.bat` to build the native libraries and copy them to their
appropriate locations in the Xamarin.Forms project. This only needs to be done
when the engine has been modified. As this is expected to be rare, it does a
release build, so it may take some time.
* Build the solution `Sudoverse.sln` in the `app` directory using Microsoft
VisualStudio.

Feel free to inspect the batch files and adapt them to your liking before
building (e.g. selecting the build targets that interest you). This way, you
may also be able to build on non-Windows platforms.

# Supported Platforms

Ideally, the app should work on UWP (Universal Windows), Android, and iOS,
however currently only UWP is actively tested. Linux and macOS may be supported
in the future.

[sudoku-variants]: https://crates.io/crates/sudoku-variants

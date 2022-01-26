cd engine
set RUSTFLAGS=-C linker=%NDK_HOME%\toolchains\llvm\prebuilt\windows-x86_64\bin\armv7a-linux-androideabi30-clang.cmd -L .\lib
cargo build --release --package engine-dynamic --target armv7-linux-androideabi
copy target\armv7-linux-androideabi\release\libengine.so ..\app\Engine\Engine.Android\lib\armeabi-v7a\libengine.so
set RUSTFLAGS=-C linker=%NDK_HOME%\toolchains\llvm\prebuilt\windows-x86_64\bin\aarch64-linux-android30-clang.cmd -L .\lib
cargo build --release --package engine-dynamic --target aarch64-linux-android
copy target\aarch64-linux-android\release\libengine.so ..\app\Engine\Engine.Android\lib\arm64-v8a\libengine.so
set RUSTFLAGS=-C linker=%NDK_HOME%\toolchains\llvm\prebuilt\windows-x86_64\bin\i686-linux-android30-clang.cmd -L .\lib
cargo build --release --package engine-dynamic --target i686-linux-android
copy target\i686-linux-android\release\libengine.so ..\app\Engine\Engine.Android\lib\x86\libengine.so
set RUSTFLAGS=-C linker=%NDK_HOME%\toolchains\llvm\prebuilt\windows-x86_64\bin\x86_64-linux-android30-clang.cmd -L .\lib
cargo build --release --package engine-dynamic --target x86_64-linux-android
copy target\x86_64-linux-android\release\libengine.so ..\app\Engine\Engine.Android\lib\x86_64\libengine.so
set RUSTFLAGS=
cargo build --release --package engine-static --target aarch64-apple-ios
copy target\aarch64-apple-ios\release\libengine.a ..\app\Engine\Engine.iOS\lib\arm64\libengine.a
cargo build --release --package engine-static --target x86_64-apple-ios
copy target\x86_64-apple-ios\release\libengine.a ..\app\Engine\Engine.iOS\lib\x86_64\libengine.a
cargo build --release --package engine-dynamic --target i686-pc-windows-msvc
copy target\i686-pc-windows-msvc\release\engine.dll ..\app\Engine\Engine.UWP\lib\x86\engine.dll
cargo build --release --package engine-dynamic --target x86_64-pc-windows-msvc
copy target\x86_64-pc-windows-msvc\release\engine.dll ..\app\Engine\Engine.UWP\lib\x86_64\engine.dll

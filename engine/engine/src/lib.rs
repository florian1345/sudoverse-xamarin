//! This is the common engine crate. It defines all functionality the engine
//! libraries must fulfill as a standard rust crate. Other creates in this
//! workspace re-export this as a static or dynamic library.

/// Returns 42. For tests that the library was loaded correctly.
#[no_mangle]
pub extern fn test() -> i32 {
    42
}

use gdnative::prelude::*;

/// The GDNative class for the Sudoku engine.
#[derive(NativeClass)]
#[inherit(Node)]
pub struct SudokuEngine;

impl SudokuEngine {
    fn new(_owner: &Node) -> SudokuEngine {
        SudokuEngine
    }
}

#[methods]
impl SudokuEngine {

    #[export]
    fn test(&self, _owner: &Node) -> i32 {
        42
    }
}

fn init(handle: InitHandle) { 
    handle.add_class::<SudokuEngine>();
}

godot_init!(init);

// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri::api::process::Command;

#[tauri::command]
fn test_process_spawn() -> Result<String, String> {
    // Test process spawning capability with echo command
    let output = Command::new("echo")
        .args(&["Process spawning works!"])
        .output()
        .map_err(|e| format!("Failed to spawn process: {}", e))?;

    // output.stdout is already a String in Tauri's Command API
    Ok(output.stdout)
}

fn main() {
  tauri::Builder::default()
    .invoke_handler(tauri::generate_handler![test_process_spawn])
    .run(tauri::generate_context!())
    .expect("error while running tauri application");
}

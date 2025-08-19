# FileCopyTool

FileCopyTool is a Windows Forms application designed to simplify copying multiple files to a specified folder. It supports multiple file-to-folder copy operations, drag-and-drop functionality (with administrator privileges), system tray integration, and a global hotkey (Ctrl+Shift+T). The application supports English (EN) and Japanese (JP) languages, with settings stored in separate configuration files for easy management.

## Features

- **Multiple Copy Rows**: Add multiple rows to specify different source files and destination folders.
- **Drag-and-Drop**: Drag files and folders into the UI (requires File Explorer to run as administrator due to UAC).
- **Browse Buttons**: Select files and folders using dialogs if drag-and-drop is unavailable.
- **Global Hotkey**: Press `Ctrl+Shift+T` to perform copy operations without focusing the app.
- **System Tray**: Minimize to the system tray and restore via double-click or hotkey.
- **Multi-Language Support**: Switch between English (EN) and Japanese (JP) via a dropdown menu.
- **Persistent Configurations**:
  - `copy-config.json`: Stores source (`From`) and destination (`To`) paths and checkbox states.
  - `settings-config.json`: Stores the selected language (EN or JP).
- **Error Handling**: Displays localized success, warning, and error messages for copy operations.

## Requirements

- **Operating System**: Windows 10 or later
- **.NET Framework**: .NET 8.0
- **Administrator Privileges**: Required for file operations and drag-and-drop (set in `app.manifest`).
- **File Explorer**: Must run as administrator for drag-and-drop to work due to UAC restrictions.

## Installation

1. **Clone or Download**:
   - Clone the repository or download the source code.
   ```bash
   git clone https://github.com/krazezt/FileCopyTool.git


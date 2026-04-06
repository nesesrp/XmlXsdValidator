# XmlXsdValidator (WinForms)

`XmlXsdValidator` is a **Windows Forms (.NET 8)** desktop application for validating an **XML** file against **two separate XSDs** and (optionally) editing the XML in a **hierarchical grid** with **live validation based on XSD constraints**.

The app validates these sections independently:
- **Header / `AppHdr`** (Header XSD)
- **Document / `Document`** (Document XSD)

## Features

- **XML viewer**: displays the selected XML on the left side in a readable format.
- **Validation with two schemas**:
  - Header XSD for `AppHdr`
  - Document XSD for `Document`
- **XML editor (TreeGrid)**:
  - load XML into the editor and edit fields in a grid.
  - parent/child hierarchy with expand/collapse.
- **XSD constraint extraction & validation**:
  - `pattern` (regex), `minLength`, `maxLength`
  - numeric limits (`minInclusive`, `maxInclusive`, etc.)
  - `enumeration` values (shown as a dropdown for enum fields)
- **Error-focused UX**:
  - invalid fields are visually highlighted in the grid.
  - right-click menu includes validation summary and filtering options.

## Requirements

- **Windows** (project targets `net8.0-windows` and WinForms)
- **.NET 8 SDK**
- (Recommended) **Visual Studio 2022** (for WinForms designer support)

## Run

### Visual Studio

1. Open `deneme.sln`
2. Set startup project to `validator`
3. Run (F5)

### Command line

```bash
dotnet --version
dotnet run --project deneme/validator.csproj
```

> Note: This is a WinForms app, so it runs on Windows.

## Usage

1. Click **XML** and select the `.xml` file to validate
2. Click **Header XSD** and select the `.xsd` for `AppHdr`
3. Click **Document XSD** and select the `.xsd` for `Document`
4. Click **Doğrula** (Validate)
   - results are printed in the “Sonuçlar” (Results) box
5. (Optional) Click **XML Editöre Yükle** (Load to editor) to open the grid editor
6. Click **Kaydet** (Save) to apply changes in memory
7. Click **Güncelenen Xml’i Gör** (View updated XML) to display the updated XML on the left

## Shortcuts & editor tips

In the editor (grid):
- **Right click**: menu (Expand/Collapse, validation summary, filtering)
- **Ctrl + +**: expand all
- **Ctrl + -**: collapse all
- **Ctrl + V**: validation summary
- **Space**: expand/collapse the selected row

## Project structure

- `deneme.sln`: Visual Studio solution
- `deneme/validator.csproj`: WinForms project
- `deneme/Form1.*`: UI and event wiring
- `deneme/UI/`: form event flows
- `deneme/Operations/`: grid/XML operations
- `deneme/Validation/`: validation based on XSD constraints
- `deneme/XsdSchemaAnalyzer.cs`: XSD analysis & constraint extraction

## Notes / limitations

- The app looks for `AppHdr` (Header) and `Document` (Document section) elements in the XML.
- Empty values are treated as “valid” in most checks (required-field rules can be added/extended if needed).


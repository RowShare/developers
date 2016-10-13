# RowShare Developers

This repository provide sample applications for the **[RowShare][rowshare] API**, a collaborative and intuitive tool to organize, share and centralize your information.

## RowShareTool
**RowShareTool** is a desktop application that allow you to browse, backup and restore your [RowShare][rowshare] tables. The tool is written using .NET 4.5 and Visual Studio but the API is a JSON/REST cross platform one.

### How to use

Add a server by clicking on `File` then `Add server ...`. Enter the server url like https://www.rowshare.com and click `OK`.

Open context menu on your server node and click on `Login`. Select a social provider and login with your credentials.
You can now browse tables in the tree view.

### Api endpoint
*Note: this is preliminary and subject to changes.*

| Url                            | Method | Description                                 |
|--------------------------------|--------|---------------------------------------------|
| /api/folder/load/{id}          | GET    | Load the folder                             |
| /api/folder/delete/{id}        | GET    | Delete the folder                           |
| /api/list/load/{id}            | GET    | Load the table                               |
| /api/list/save                 | POST   | Add/update the table. See [List][list]      |
| /api/list/delete : {Id:id}     | POST   | Delete the table                            |
| /api/column/loadforparent/{id} | GET    | Load columns definition                     |
| /api/column/save               | POST   | Add/update the column. See [Column][column] |
| /api/row/loadforparent/{id}    | GET    | Load table's rows                           |
| /api/row/load/{id}             | GET    | Load the row                                |
| /api/row/save                  | POST   | Add/update the row See [Row][row]           |
| /api/row/deletebatch           | POST   | Add/save a row in batch mode. See [Row][row]         |

[rowshare]: https://www.rowshare.com
[list]: RowShareTool/Model/List.cs
[column]: RowShareTool/Model/Column.cs
[row]: RowShareTool/Model/Row.cs

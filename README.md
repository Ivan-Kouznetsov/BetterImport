# Better Import 
 
Traditionally, importing from text files into SQL Server is done using the bulk copy program (bcp)
an effective but rather limited and user-unfriendly program - skipping columns via bcp, for example, requires editing a text format file which is so fragile that bcp will error out if the format file does not end
in an empty line. It also has no tolerance for bad data, which is virtually guaranteed when importing  
from large 3rd-party CSVs or TSVs.

BetterImport has a simple interface for mapping columns, uses JSON for configuration, and skips and logs bad data.
# Usage

dotnet BetterImport.dll job.json

job.json format:  
```
{
  "ConnectionString": "Data Source=My-PC;Initial Catalog=MyDatabase;Integrated Security=True",
  "TableName": "MyTable",
  "DataFile": "D:\\Path\\To\\File.tsv",
  "ValueSeparator": "\t",
  "SkippedRowFile": "skippedrows.txt",
  "ErrorLogFile": "errors.txt",
  "StartRow": 2,
  "Columns": [
    {
      "Name": "Id",
      "DataFileIndex": 2
    },
    {
      "Name": "Text",
      "DataFileIndex": 3
    }
	,
    {
      "Name": "Number",
      "DataFileIndex": 5
    }
  ]
}
```
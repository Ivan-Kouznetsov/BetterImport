# Better Import 
 
Traditionally, importing from text files into SQL Server is done using the bulk copy program (bcp)
an effective but rather limited and user-unfriendly program - skipping columns via bcp, for example,
requires editing a text format file which is so fragile that bcp will error out if the format file does not end
in an empty line. It also has no tolerance for bad data, which is virtually guaranteed when importing  
from large 3rd-party CSV or TSV files.

BetterImport has a simple interface for mapping columns, uses JSON for configuration, skips and logs bad data, and can preprocess data before importing.
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

# Optional Preprocessing

Sometimes the data you want to import has inconsistencies  or magic numbers that you want to fix before importing. 
For example, if you want to import claim.tsv from (http://www.patentsview.org/download/) you will find that (at least at the time of this writing):
* Some `text` entries start with the claim number, some do not 
* The `dependent` column uses -1 to indicate none.

If you want wanted to remove claim numbers from the start of all claim text entries and replace -1 in the dependant column with NULL using BetterImport you could 
add the following to the job.json file:

```
  "Preprocessors":
  [
	{"DataFileIndex": 3,
	"Pattern":"^\\d+\\.",
	"ReplacementValue": ""},
	{"DataFileIndex": 4,
	"Pattern":"-1",
	"ReplacementValue": "NULL"}
  ]
```

Note that \ has to be escaped so \d becomes \\d.

The complete file could look like this:

```
{  
  "ConnectionString": "Data Source=HOSTNAME;Initial Catalog=TABLENAME;Integrated Security=True",
  "TableName": "Patents",
  "DataFile": "D:\\Path\\To\\File.tsv",
  "ValueSeparator": "\t",
  "SkippedRowFile": "skippedrows.txt",
  "ErrorLogFile": "errors.txt",
  "StartRow": 2,
  "Columns": [
    {
      "Name": "PatentNumber",
      "DataFileIndex": 2,
	  
    },
    {
      "Name": "ClaimText",
      "DataFileIndex": 3
    }
	,
    {
      "Name": "DependentIdList",
      "DataFileIndex": 4
    }
	,
    {
      "Name": "ClaimNumber",
      "DataFileIndex": 5
    }
	,
    {
      "Name": "IsExemplary",
      "DataFileIndex": 6
    }
  ],
  "Preprocessors":
  [
	{"DataFileIndex": 3,
	"Pattern":"^\\d+\\.",
	"ReplacementValue": ""},
	{"DataFileIndex": 4,
	"Pattern":"-1",
	"ReplacementValue": "NULL"}
  ]
}
```

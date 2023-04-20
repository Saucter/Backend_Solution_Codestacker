# Introduction
Welcome to my project! My name is Mohammed Al-Hosni, 17, and at the time of writing a student at RGOTC.
This repo is a proposed solution for Rihal's backend developement challenge. A part of their Codestacker 2023 contest.

> Here is a [Notion file](https://traveling-flame-6a3.notion.site/PDF-API-codestacker-challenge-306ee59ee3be419b9b339fd80a53c1ea
) showcasing the notes written during the preperation phase for the challenge: 


# Running instructions
### GitHub or zip file installation
* `Git install`: Clone the repo using `git clone https://github.com/Saucter/Backend_Solution_Codestacker.git`
* `Zip file install`: Simply download and unzip the file to a given directory 
* Launch the cloned repository / Unzipped file on your choice of Text Editor or IDE
* Launch a new terminal session
* Run `cd ./PDF_Reader_APIs/Server` (Use backward slashes for Windows)
* Run `dotnet run` 
* Click the link that shows up in the terminal while holding the `Cntrl` or `⌘` key for Windows/Linux or MacOS respectively

### Docker installation
* Yes


# API URIs
### Post URI(S)
**PostPDF:** http://localhost:5143/pdf/PostPDF
Allows the user to post a PDF to the database
* Param 1: `Files`: form-data submissions to upload a PDF
* Param 2: `bool WithTxtFile`: If true inputs the parsed sentences into a .txt file and uploads it to object storage. False by default and can be disabled to improve speed
* Param 3: `bool WithImages`: If true runs OCR over the images of the PDF. Used for Image-based PDFs. False by default, can be disabled to improve speed

### GET URI(S)
**PostPDF:** http://localhost:5143/pdf/PostPDF
Allows the user to post a PDF to the database
* Param 1: `Files`: form-data submissions to upload a PDF
* Param 2: `bool WithTxtFile`: If true inputs the parsed sentences into a .txt file and uploads it to object storage. False by default and can be disabled to improve speed
* Param 3: `bool WithImages`: If true runs OCR over the images of the PDF. Used for Image-based PDFs. False by default, can be disabled to improve speed

**PostPDF:** http://localhost:5143/pdf/PostPDF

**PostPDF:** http://localhost:5143/pdf/PostPDF

### DELETE URI(S)
**PostPDF:** http://localhost:5143/pdf/PostPDF
Allows the user to post a PDF to the database
* Param 1: `Files`: form-data submissions to upload a PDF
* Param 2: `bool WithTxtFile`: If true inputs the parsed sentences into a .txt file and uploads it to object storage. False by default and can be disabled to improve speed
* Param 3: `bool WithImages`: If true runs OCR over the images of the PDF. Used for Image-based PDFs. False by default, can be disabled to improve speed

**PostPDF:** http://localhost:5143/pdf/PostPDF


# Tools utilized
### Tech stack 
* Language: C#
* Framework: ASP.NET Core
* Database: MSSQL / SQL Server
* DB Mapper: Entity Framework Core
* DB Storage: Azure SQL DB
* Object storage: Azure Object Storage

### Additional tools utilized
* Containerization: Docker
* Version Control: Git and GitHub (Pretty sure you already knew that lol)
* API testing: Postman
* RegEx testing: RegExer

### NuGet Packages
* Microsoft.EntityFrameworkCore (And its related sub-libraries)
* Azure.Storage.Blobs
* Spire.Pdf
* Tesseract (And its related sub-libraries)
* dotnet-stop-words
> [The full list can be viewed here](PDF_Reader_APIs/Server/PDF_Reader_APIs.Server.csproj)

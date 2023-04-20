# Introduction
Welcome to my project! My name is Mohammed Al-Hosni, 17 and studying at RGOTC at the time of writing.

This is my submission for Rihal's backend development challenge as part of the Codestacker 2023 contest. This repository contains a PDF reader API built using C# and ASP.NET Core, which allows users to interact with a database of PDFs.

If you'd like to check out the notes I took during the preparation phase for the challenge, you can find them in this [Notion file](https://traveling-flame-6a3.notion.site/PDF-API-codestacker-challenge-306ee59ee3be419b9b339fd80a53c1ea).

With that being said, the installation instructions and API endpoints are as follows:

## Appraoch
My approach to tackling the challenge and developing the API was to focus on creating a production-ready solution while keeping the user in mind. To achieve this, I utilized C# and ASP.NET Core as the primary language and framework, respectively, due to my familiarity with these tools especially due to my heavy use of them currently while working on my graduation project. The API was built with MSSQL / SQL Server as its database, utilizing Entity Framework Core as its mapper for ease of integration with ASP.NET Core. The API was designed to meet the requirements of the challenge stated in the initial brief sent to the contestents while also incorporating additional features to enhance the user's quality of life. The main features still remain the upload of PDF onto a datbase, retrieval of a PDF's information, returning sentences containing a given keyword, returning the most frequent words in a PDf, and deleting PDFs based on a given ID. However, certain ideas were added as to improve the user-experience and the program's overall efficiency such as giving freedom of choice during the posting process and utilizing caching.

To achieve these functionalities, the API utilizes various NuGet packages such as Microsoft.EntityFrameworkCore and Azure.Storage.Blobs for database and object storage, respectively. Other packages used include Spire.Pdf and Tesseract for PDF and OCR processing, and dotnet-stop-words for filtering stop words. In terms of security, the API requires authentication for certain actions, with specific usernames and passwords provided. Hope my solution is to a satisfactory standard!

## Installation
* Clone the repo using git clone https://github.com/Saucter/Backend_Solution_Codestacker.git or simply download and unzip the file to a given directory.
* Ensure that you have Dotnet SDK 7.0 or later installed.
* Launch the cloned repository / unzipped file on your choice of Text Editor or IDE.
* Launch a new terminal session.
* Run cd ./PDF_Reader_APIs/Server (Use backward slashes for Windows).
* Run dotnet run.
* Click the link in the terminal while holding the Cntrl or ⌘ key for Windows/Linux or MacOS respectively.
* The API can then be tested using Postman or Insomnia using the shown API URIs.

## API Endpoints
### POST URI(s)
**PostPDF:** http://localhost:5143/pdf/PostPDF

Allows the user to post a PDF to the database
* Param 1: `form-data Files`: A form-date parameter for PDF submission / upload
* Param 2: `bool WithTxtFile`: If true inputs the parsed sentences into a .txt file and uploads it to object storage. False by default and can be disabled to improve speed
* Param 3: `bool WithImages`: If true runs OCR over the images of the PDF. Used for Image-based PDFs. False by default, can be disabled to improve speed

### GET URI(s)
**GetPDFs:** http://localhost:5143/pdf/GetPDFs

Allows the user to retireve a PDF and its information based on the PDF's ID
* Param 1: `List<int>? id`: A list of IDs for the API to retrieve from the DB. Retrieves all PDFs if the list is null
* Param 2: `bool WithSentences`: If true it returns the PDFs with the parsed sentences. False by default, turned off to minimize screen clutter on retrieved JSON if unnecessary

**GetKeyword:** http://localhost:5143/pdf/GetKeyword

Allows the user to retrieve the sentences in which a given keyword resides in as well the number of times the word has occurred
* Param 1: `List<int>? id`: Allows the user to decide which PDF(s) to look in. Looks at all PDFs if the list is null
* Param 2: `string Keyword`: the keyword that the API should look for
* Param 3: `bool Exact`: If true it makes sure the matched results are exactly like the submitted keyword (i.e. 'and' in 'mandate' is not considered). False by default
* Param 4: `bool CaseSensitive`: If true it makes sure the matched results follow the same case as the submitted keyword. False by default

**GetTopWords:** http://localhost:5143/pdf/GetTopWords

Gets the top 'x' words in a list of PDFs while filtering out any stop-words
* Param 1: `List<int>? id`: A list of PDF IDs that the API should look for top words in. Checks the top words in all PDFs if the list is null
* Param 2: `int? NumberOfWords`: Allows the user to the top 'x' words to retrieve. If param is null the default the top 5 words. 
* Param 3: `List<string>? Ignore`: Allows the user to remove certain words from the retrieved list

### DELETE URI(s)
**DeletePDF:** http://localhost:5143/pdf/DeletePDF

Alllows the user to delete an PDF from the DB based on ID. Requires header authentication
* Param 1: `List<int>? id`: A list of PDF IDs that the API should delete from the DB

**DeleteAll:** http://localhost:5143/pdf/DeleteAll

Alllows the user to delete all PDFs found in the DB. Requires special header authentication
* No parameters

### Authentication (Basic)
* Username: Mohammed | PW: DoesThisWork123?
* Username: RihalTeam | PW: Your_PW:)
* FOR DELETE APIs -> Username: ForDeleteOnly | PW: PlsDoNotUse101

# Utilized tools
### Tech stack 
* Language: C#
* Framework: ASP.NET Core
* Database: MSSQL / SQL Server
* DB Mapper: Entity Framework Core
* DB Storage: Azure SQL DB
* Object storage: Azure Object Storage

### NuGet Packages
* Microsoft.EntityFrameworkCore (And its related sub-libraries)
* Azure.Storage.Blobs
* Spire.Pdf
* Tesseract (And its related sub-libraries)
* dotnet-stop-words
> [The full list can be viewed here](PDF_Reader_APIs/Server/PDF_Reader_APIs.Server.csproj)
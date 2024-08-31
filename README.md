# monument
[![Azure Static Web Apps CI/CD](https://github.com/aluitink/monument/actions/workflows/azure-static-web-apps-wonderful-sea-01070661e.yml/badge.svg)](https://github.com/aluitink/monument/actions/workflows/azure-static-web-apps-wonderful-sea-01070661e.yml)

[![.NET code metrics](https://github.com/aluitink/monument/actions/workflows/code-meteics.yml/badge.svg)](https://github.com/aluitink/monument/actions/workflows/code-meteics.yml)

[![CodeQL](https://github.com/aluitink/monument/actions/workflows/codeql.yml/badge.svg)](https://github.com/aluitink/monument/actions/workflows/codeql.yml)

Grief and the grieving process is difficult and personal to anyone experiencing it. When I had to say goodbye to my dog, I spent a lot of time remembering her and looking through pictures. 

Although it is good to process and embrace these feelings, I needed a break from the thoughts and wanted something to focus that energy on.

Monument is an Azure Static Web App backed by a storage account.

I idea is to allow creation of pages and content via Markdown.

The Blazor WASM UI will interact with an Azure Functions API to access and modify files stored as JSON. The JSON documents will act as the content, describing what will be seen.

I have added some ARM templates for the infrastructure linked to this Deploy button.

Once your infrastructure is deployed, you can link your deployment token to your cloned repo and CI/CD will deploy updates.

I am putting this together with various chunks of code from other projects, looking for a minimal viable solution. I would be thrilled if this project gained interest of others.

I have no monetary goals with this, my only wish is for anyone wanting a way to remember a loved one, to be able to click, deploy, and customize.


[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Faluitink%2Fmonument%2Fmain%2Finfra%2Fazuredeploy.json)

Today I added a codespace config - you should be able to spin a codespace and access via SWA Emulator

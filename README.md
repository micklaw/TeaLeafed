# Umbraco Migrate & Import Sample

## Introduction

I used this code for a project I worked on recently, I was going to package it up, but alas, paid work, kids, illness... life, got in way! So best share it with someone who it may be of use too.

The idea behind the original concept of the code was too provide a way to **migrate** a listing of posts or blogs from one site to another, on a first pass scan a listing page to find all the child pages contained with in it. 
On this page we can pull information of each child page like its link, title, image, summary text etc. We can then do a second pass on the pulled information to pull associated information from their individual page details, like
hero images, quotes, authors, author images, comments etc.

The code also provides a means to just do the second pass above on a collection of urls, so no need for the listing pass if we already have the collection of urls we need.

Finally, we then **import** this information to umbraco. This is also achieved in this code.

## Configuration

The process makes use of an options.json file per teplate to import. This file defines things like, the document type to save the content as, properties to map to via XPath (more info to follow), should we make a url 
rewrite file?, the repeating container item xpath for child pages in the listing, the parent node Id to save found pages against etc.

The file is saved in a config defined folder '/app_data/[folder-name]/[setup-guid]' where the guid is unique per setup. I have no UI for this, but i envisaged the options.json file would be created in the UI and then 
serialized to disk, giving a quick CRUD of the file in the UI on any previously ran setups?

### Property configuration

Now, the confusing bit. This process makes use of XPath to map the content to pull for your results. XPaths not confusing I hear you cry, and your right, but what to do with the Xpath value can be. So in this instance the
code has a concept of **migrator converters** and **importer converters**. Migrator converters are ways to register methods which will parse the DOM for specific values, so anyone can easily write your own, and register them
to grab things from the DOM in any way they like. For example, I have inner HTML converter, inner Text converter, attribute value converter, meta tag converters etc. These are registered on startup to its related container and then defined
against a property in the options.json file so so this method is then invoked to parse the DOM.

Importing has the exact same concept so you can roll your own media importer, archetype importer etc when importing the found property values. If no importer is defined on a property, we just attempt to set the value of 
the umbraco property using its deserialized type.

#### Sample migrator converter

```csharp
public static Func<MigrationOptions, HtmlDocument, HtmlNode, bool, string, string> InnerText = (options, document, node, useRelative, xPath) =>
{
    var root = ValidateDefaults(document, node, useRelative);

    var element = root.SelectSingleNode(xPath);

    if (element != null)
    {
        var value = element.InnerText;

        return (value ?? string.Empty).Trim('\r', '\n', ' ');
    }

    return null;
};
```

In the sample project attached I register these in my container, but it could easily be done in the global.asax or IApplicationStarting events.

```csharp
ConverterResolver.Init();
ConverterResolver.Register("html", MigrationConverters.InnerHtml);
ConverterResolver.Register("text", MigrationConverters.InnerText);
ConverterResolver.Register("attribute", MigrationConverters.AttributeValue);
```

On invoking these methods, some objects are auto passed as parameters, but in the options.json properties there are converter args arrays, you can pass values expected on your converters e.g parnt node ids, booleans etc.

#### Sample **options.json** file

```json
{
   "CreateRewriteFile":false,
   "Key":"0e1bc703-cf5d-47a9-b82d-1afeb78ba97b",
   "Description":"Listing pages for the rockspring website on news and research e.g. http://www.rockspringpim.com/news-and-research/press-releases/2016.aspx",
   "Scheme":"http",
   "Domain":"www.rockspringpim.com",
   "RepeatingItemXPath":".//div[contains(@class,'cat-wrap')]/div",
   "ListingMappings":[
      {
         "DefaultValue":null,
         "PropertyAlias":"url",
         "ConverterAlias":"attribute",
         "ConverterArgs":[
            true,
            ".//h3/a",
            "href"
         ],
         "Value":null,
         "Format":"http://www.rockspringpim.com{0}",
         "ImporterAlias":null,
         "ImporterArgs":null,
         "UseAsName":false
      },
      {
         "DefaultValue":null,
         "PropertyAlias":"title",
         "ConverterAlias":"text",
         "ConverterArgs":[
            true,
            ".//h3/a"
         ],
         "Value":null,
         "Format":null,
         "ImporterAlias":null,
         "ImporterArgs":null,
         "UseAsName":true
      },
      {
         "DefaultValue":null,
         "PropertyAlias":"introduction",
         "ConverterAlias":"text",
         "ConverterArgs":[
            true,
            ".//p"
         ],
         "Value":null,
         "Format":null,
         "ImporterAlias":null,
         "ImporterArgs":null,
         "UseAsName":false
      }
   ],
   "DetailsMappings":[
      {
         "DefaultValue":"2015-12-11T01:11:35.8876826+00:00",
         "PropertyAlias":"datePublished",
         "ConverterAlias":null,
         "ConverterArgs":null,
         "Value":null,
         "Format":null,
         "ImporterAlias":null,
         "ImporterArgs":null,
         "UseAsName":false
      },
      {
         "DefaultValue":true,
         "PropertyAlias":"feature",
         "ConverterAlias":null,
         "ConverterArgs":null,
         "Value":null,
         "Format":null,
         "ImporterAlias":null,
         "ImporterArgs":null,
         "UseAsName":false
      },
      {
         "DefaultValue":null,
         "PropertyAlias":"image",
         "ConverterAlias":"attribute",
         "ConverterArgs":[
            false,
            ".//body/div[@id='wrapper']/div[@id='main']/div[@id='content']/div[@class='img']/img",
            "src"
         ],
         "Value":null,
         "Format":"http://www.rockspringpim.com{0}",
         "ImporterAlias":"image",
         "ImporterArgs":[
            1081
         ],
         "UseAsName":false
      }
   ],
   "ParentNodeId":1075,
   "DocumentTypeAlias":"BlogPost"
```

## Running it...

So in summary, once you have setup your options file and your converters away you go. Unfortunately as we have no UI, this has to be achived by placing options.json files in the guid folders and simply calling a few Urls
in sequence. This will obviously be automated with a UI.

###£ Process 1: Listing page, get is child pages (option.json id: 0e1bc703-cf5d-47a9-b82d-1afeb78ba97b)

1. Hit the url to process the listing page: /umbraco/backoffice/api/migration/GetListings?key=0e1bc703-cf5d-47a9-b82d-1afeb78ba97b&url=http://www.rockspringpim.com/news-and-research/press-releases/2016.aspx
2. Hit the url to grab found pages details: /umbraco/backoffice/api/migration/GetListingsComplete?key=0e1bc703-cf5d-47a9-b82d-1afeb78ba97b
3: Hit url to import: /umbraco/backoffice/api/migration/GetImport?key=0e1bc703-cf5d-47a9-b82d-1afeb78ba97b

###£ Process 1: Listing page, get is child pages (option.json id: 345435435-c345d-445c-34cd-cvrv545ba97b)

1. Post to url with an array of url {[url1,url2,url3]}: /umbraco/backoffice/api/migration/GetDetails?key=345435435-c345d-445c-34cd-cvrv545ba97b
3: Hit url to import: /umbraco/backoffice/api/migration/GetImport?key=345435435-c345d-445c-34cd-cvrv545ba97b

## Step by Step

The reason behind doing it with multiple Urls is so a user could do each stage independently without committing to importing content to Umbraco. After each crawl of data a results.json file is saved with the results of the run.
This file can then be presented and updated manually by the user, of whatever really before importing. Migrations can be a fiddly business so thought this would be neccassary i.e XPath incorrect etc.

## **Disclaimer**

This project was knocked up based on a couple of days of hacking for a project I completed not so long ago, so is very rough around the edges, but at worst it might be good for some ideas. The request are all async so run very quickly
based on the implementation I used for my project. It made use of collating and then importing media, archetypes, nodes, dates, numerics, basically lots of different data types and worked well. So if there are many code
smells, don't bag me too much ;)

## Sample Solution

There is a sample website here as well as the importer / migrations code in a seperate assembly for ripping out. The username and password for the CMS is unimaginatvely 'admin' and 'password'.


Hope it can be of use to you Tim, or anyone that wants it really.

Cheers

Michael


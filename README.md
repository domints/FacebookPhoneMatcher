# FacebookPhoneMatcher
Simple .NET Core app matching Google CSV with your contacts to data from recent (2021) Facebook phone number leak.

Google CSV can be obtained easily if you store your phonebook on Google Account, by going to https://contacts.google.com/ and clicking Export.
As for the leak, well, it's up to you :)

App matches only first phone number for given contact from that list, that might change.
It also doesn't like multiline CSV rows (it's meant to be simple!) so you might want to go through your CSV looking for `"` char and replacing new lines, e.g. in addresses with something nice, like spaces.
That also might change in the future (if somebody will implement better CSV parsing, or I will have enough will to do this).

It literally is 10 minute app, so don't expect much from it, but it does its job good enough.

Oh, you'll see output in the console. Like, name you see in your phone, then phone number, then their Facebook name. If you need something more (it could probably give you Facebook profile link?), well, you know how to C#, don't you?

As for the usage:

```
You need three args: country code, contacts file name and leak file name!
Usage: dotnet run -- countryCode contactsFile leakFile
For example: dotnet run -- 48 contacts.csv Poland.txt
contacts.csv is contacts exported from Google Contacts, as Google CSV.
```

App will also remind you if you won't remember.

Requirements: 
 - not that much. .NET Core 5.0, at least
 - data files might be useful as well.

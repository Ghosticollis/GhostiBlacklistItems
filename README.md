# GhostiBlacklistItems
Unturned blacklist items/vehicles spawn

this module will prevent blacklisted items/vehicles from spawning in your server.
for example, you could prevent Stealy Wheely from spawning, prevent raid gear or prevent stuff like drugs, alcohol ... etc
also you could use to prevent specific vehicles from spawning.


- please note:
this is a module, not a rocket mod plugin
it is the same idea, but this run on any server, even if it doesn't have rocket mod


- please note 2: 
it is very recomended to compile it by your own. to do that you need to compile the cs files as a .Net framework class library (more info you could search tutorials how to create Unturned Module)
in case you don't want to get your own build, I have supplied a compiled and ready to use build.


# how to setup:
copy the folder `GhostiBlacklistItems` to your Unturned server folder and put it under the folder `Modules`



# how to use:
add items id to the config file `/Module/GhostiBlacklistItems/config.txt`
and that's it


# extra:
this module has extra features that could be very helpful for map designers. (if you are not map designer you could ignore this section).
you could add this module to your local server that running your map, then you could use the following commands from console:

(please note: when you planning to use this commands it is recomended to set "enable filtering" to "false" at the module config file)
```
scan for item [item_id]
recursive scan for item [item_id]
scan for table [table_id]
print vehicle spawn tables
scan for empty tables
print spawn table items [table_id]
```
scan for item [item_id] - this command will print to you spawn tables that spawn the specified item
example to show the difference between normal and recursive:
let mili_ST be a spawn table that only contains mili_clothes spawn table, and mili_guns spawn table.
if u search specific gun using first command you will get: mili_guns
but if u use second command (recursive search) you will get: mili_ST and mili_guns

scan for table [table_id] - this command is very useful if you encounter error similar to
`Unable to find spawn table for resolve with id 5250`
if you use command "scan for table 5250" you will get where you misinput the values
for example in this case it was "Fishing zombie", so simply you go to Fishing zombie, and re-enter ID of valid spawn table.



Greetings

Ghosticollis @ SOD servers discord

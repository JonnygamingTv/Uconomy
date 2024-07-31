# Uconomy
A simple currency mod that creates a single table in your database, this table will store the players currency amount and others mods can easily manipulates that and been very compatible.

There are 2 commands: /balance, and /pay.
/balance and /pay are meant to be used by everyone that has permissions for them. Just put the command in the permissions file for the group.

Usage:
/balance: This will show the player balance.
/pay <player> <amount>: Trasnfer a specific amount of your balance to another player

Configuring:
Don't forget to add the commands /balance and /pay into ``Permissions.config.xml``
```xml
<Group>
  <Id>default</Id>
  <DisplayName>Guest</DisplayName>
  <Prefix />
  <Suffix />
  <Color>white</Color>
  <Members />
  <Priority>100</Priority>
  <Permissions>
    <Permission Cooldown="0">balance</Permission>
    <Permission Cooldown="0">pay</Permission>
  </Permissions>
</Group>
```

# Building

*Windows*: The project uses dotnet 4.8, consider installing into your machine, you need visual studio, simple open the solution file open the Build section and hit the build button (ctrl + shift + b) or you can do into powershell the command dotnet build -c Debug if you have installed dotnet 4.8.

*Linux*: Unfortunately versions lower than 6 of dotnet do not have support for linux, the best thing you can do is install dotnet 6 or the lowest possible version on your distro and try to compile in dotnet 6 using the command dotnet build -c Debug, this can cause problems within rocket loader.

FTM License.

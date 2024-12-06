@echo off

echo Starting MongoDB instances...

start cmd /k "mongod --dbpath C:\Projects\NumismaticClub\CoinsDb"

start cmd /k "mongod --dbpath C:\Projects\NumismaticClub\UsersDb"

echo Both MongoDB instances are starting in separate terminals.

pause
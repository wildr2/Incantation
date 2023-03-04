@echo off
setlocal
cd /D "%~dp0"
cd ..
@echo on
python -m magic_server.src.magic_server

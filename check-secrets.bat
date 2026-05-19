@echo off
echo Check secrets before commit
git diff --cached | findstr /i "password secret key token jwt env"
if %errorlevel%==0 (
    echo Secrets founded.Don't commit
    exit /b 1
) else (
    echo Secret's not found. You can commit.
)
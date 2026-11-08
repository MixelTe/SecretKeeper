```
Description:
  SecretKeeper:Exractor - Console utility for extracting passwords and generating TOTP codes.

  I/O Communication:
        - Passwords/TOTP secrets are written directly to stdout (w/o EOL).
        - Errors are written to stderr.
        - Master passwords for encrypted ZIPs can be piped via stdin or entered interactively.

Usage:
  SecretKeeper [options]

Options:

  -d, --folder <folder>    Путь к папке, в которой находится файл данных
  -f, --file <file>        Имя файла данных
  -a, --archive <archive>  Имя файла архива, в котором находится файл данных
  -p, --param <param>      Имя искомого параметра [default: pwd]
  -s, --section <section>  Имя раздела, в котором нужно искать параметр
  -?, -h, --help           Show help and usage information
  --version                Show version information
```

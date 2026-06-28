# SecretKeeper

## Vault Structure

```
vault_folder/
├── name.txt
│	├── pwd: <VALUE>
│	├── otp: <VALUE>
│	└── anykey: <VALUE>
├── grandma.txt
│	├── --- mail ---
│	│	├── login: <VALUE>
│	│	├── pwd: <VALUE>
│	│	└── phone: <VALUE>
│	└── ---    AnoTHer  seCTion  ---
│		├── pwd: <VALUE>
│		└── anykey: <VALUE>
├── my_mail.txt
├── wifi_pwd.txt
├── encrypted.zip
│	├── enc_pwd.txt
│	├── wifi_pwd.txt
│	└── my_mail.txt
├── encrypted2.zip
│	└── wifi_pwd.txt
└── some_pwd.zip
	└── anyname.txt

another_vault/
├── my_mail.txt
├── other_name.txt
└── some_pwd.zip
	└── any_name.txt
```

## Get

```
sek <absolute path> [--file <file>] [--key <key>] [--section <section>] [--action <action>]
sek get <name> [--key <key>] [--section <section>] [--action <action>]
sek get <name> [-k <key>] [-s <section>] [-a <action>]
```
* `name` – txt file name
master password is requested for encrypted files

Resolution:

```text
name           -> find unique match
/name          -> find in any vault root
/zipname/name  -> find in zip in any vault
vault/name     -> specific vault root
vault/zip/name -> specific vault zip
vault/*/name   -> find unique match in specific vault
```

Options:

```text
--file, -f      file in zip (default: any if one found in zip else error)
--key, -k       key (default: pwd)
--section, -s   section (default: *nameless*)
--action, -a    no | otp (default: no, otp for key=otp)
```

Examples:

```
# get pwd from vault_folder/name.txt
sek get name

# get other keys
sek get name -k otp
sek get name -k anykey


# get pwd from vault_folder/encrypted.zip/enc_pwd.txt
sek get enc_pwd
> enter master password:

# get pwd from mail section of vault_folder/grandma.txt
sek get grandma -s mail

# get pwd from "    AnoTHer  seCTion  " section of vault_folder/grandma.txt
sek get grandma -s another_section


# same name in different zips
sek get wifi_pwd  # ERROR: name is ambiguous

# get pwd from vault_folder/wifi_pwd.txt
sek get /wifi_pwd

# get pwd from encrypted.zip/wifi_pwd.txt
sek get /encrypted/wifi_pwd

# get pwd from encrypted2.zip/wifi_pwd.txt
sek get /encrypted2/wifi_pwd


# same name in different folders
sek get my_mail  # ERROR: name is ambiguous

# get pwd from vault_folder/my_mail.txt
sek get vault_folder/my_mail

# get pwd from vault_folder/encrypted.zip/my_mail.txt
sek get vault_folder/encrypted/my_mail

# get pwd from another_vault/my_mail.txt
sek get another_vault/my_mail
```

## Vaults

```
sek vault add <path to folder> [--primary] [--alias <alias>]
sek vault add <path to folder> [-p] [-a <alias>]
sek vault remove <path to folder>
sek vault list
```

| alias         | path               | primary |
|---------------|--------------------|---------|
| vault_folder  | .../vault_folder/  | yes     |
| another_vault | .../another vault/ | no      |

## Add

```
sek add <name> <value> [--key <key>] [--section <section>] [--overwrite]
sek add <name> <value> [-k <key>] [-s <section>] [-o]
```

Master password is requested for zips

Options:

```text
--key, -k       key (default: pwd)
--section, -s   section (default: *nameless*)
--overwrite, -o allow overwrite
```

Examples:

```text
# add a new entry to the primary vault
sek add teapot 12345

# add a new entry to specific vault
sek add another_vault/teapot 12345

# add a new entry to the primary vault with a key and section
sek add kitchen 12345 -k otp -s teapot

# add a new entry to zip in the primary vault
sek add /kitchen/teapot 12345
> enter master password:

# add a new entry to zip in specific vault
sek add another_vault/kitchen/teapot 12345
> enter master password:
```

## Remove

```
sek remove <name> [--key <key>] [--section <section>]
sek remove <name> [-k <key>] [-s <section>]
```

* Master password is requested for zips
* Empty files are deleted.

Options:

```text
--key, -k       key to remove (default: pwd)
--section, -s   section (default: *nameless*)
--pwd, -p       zip password; if omitted, master password is requested
```

## List
```text
sek list [--all | -a]
```

Lists all entries.

`--all` requests the master password and includes encrypted entries.

<br>

| name                              | full_name                         | is_encrypted | sections | keys              |
|-----------------------------------|-----------------------------------|--------------|----------|-------------------|
| grandma                           | vault_folder/grandma              | false        | mail, another_section | mail:login, mail:pwd, mail:phone, another_section:pwd, another_section:anykey |
| name                              | vault_folder/name                 | false        |          | login, pwd, phone |
| enc_pwd                           | vault_folder/encrypted/enc_pwd    | true         |          | pwd               |
| /wifi_pwd                         | vault_folder/wifi_pwd             | false        |          | pwd               |
| /encrypted/wifi_pwd               | vault_folder/encrypted/wifi_pwd   | true         |          | pwd               |
| vault_folder/my_mail              | vault_folder/my_mail              | false        |          | pwd               |
| /encrypted/my_mail                | vault_folder/encrypted/my_mail    | false        |          | pwd               |
| another_vault/my_mail             | another_vault/my_mail             | false        |          | pwd               |
| vault_folder/some_pwd             | vault_folder/some_pwd/anyname     | true         |          | pwd               |
| another_vault/some_pwd            | another_vault/some_pwd/any_name   | true         |          | pwd               |

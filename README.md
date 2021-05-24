# SharpRDPDump
 Create a minidump of TermService for clear text pw extraction
all credits go to JonasLyk
```
 ._________________.
 | _______________ |
 | I             I |
 | I    RDPDump  I |
 | I    @jfmaes  I |
 | I             I |
 | I_____________I |
 !_________________!
    ._[_______]_.
.___|___________|___.
|::: ____           |
|    ~~~~ [CD-ROM]  |
!___________________!



 Usage:
  -h, -?, --help             Show Help


  -l, --location=VALUE       the location to write the minidumpfile to
                               (default:%localappdata%\rdpdump.bin)
  -c, --compress             compressess the minidump and deletes the normal
                               dump from disk (gzip format)
                               
                             
```



```
SharpRDPDump.exe -c


 ._________________.
 | _______________ |
 | I             I |
 | I    RDPDump  I |
 | I    @jfmaes  I |
 | I             I |
 | I_____________I |
 !_________________!
    ._[_______]_.
.___|___________|___.
|::: ____           |
|    ~~~~ [CD-ROM]  |
!___________________!



termservice found, PID: 1736
[+] Dump successful!

[*] Compressing C:\Users\xxx\AppData\Local\rdpdump.bin to C:\Users\xxx\AppData\Local\rdpdump.bin.gz gzip file
[X] Output file 'C:\Users\xxx\AppData\Local\rdpdump.bin.gz' already exists, removing
[*] Deleting C:\Users\xxx\AppData\Local\rdpdump.bin

[+] Dumping completed.
 check C:\Users\xxx\AppData\Local\rdpdump.bin.gz
```

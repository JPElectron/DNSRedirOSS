# DNSRedirOSS

This readme is for DNS Redirector OSS

Previous versions of this software were licensed via the website www.dnsredirector.com

DNS Redirector offers Captive portal, Whitelist-only Internet, and Internet filtering features - you can implement any combination of these features.  The software is intended to run on Windows servers - as a recursive DNS service between client/end-user devices on the LAN and your AD DNS service (the Microsoft DNS service) or a 3rd party DNS server, such as your ISP, Google DNS, etc.

The latest version from GitHub is: v7.2.0.18 r10/06/2020 (requires .NET Framework 4.6.1)

You can download it here: https://github.com/JPElectron/DNSRedirOSS/raw/main/dnsredir-download.zip

    You MUST unblock this package before extracting
    Right-click on the dnsredir-download.zip you downloaded, select Properties, click the Unblock button
       ...if this button is not present just proceed, click OK
    Extract the .zip contents to C:\DNSREDIR (do NOT replace your .ini files if you are upgrading)
    In the C:\DNSREDIR folder, right-click on _setup_.bat and select "Run as administrator"
       ...this wizard will assist with 1st time setup (no need to run this when upgrading)


## Documentation

The latest ReadMe file is here: [full-ReadMe](full-ReadMe.md)

The latest FAQ file is here: [full-FAQ](full-FAQ.md)
 
Sample files and other resources are here: https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx


## The Future

While DNS Redirector is still needed for a successful captive portal implementation, be aware of the new method to impose a captive portal by using DHCP option 114, for devices that support it.  More info here: https://tools.ietf.org/id/draft-ietf-capport-rfc7710bis-08.html and here: https://developer.android.com/about/versions/11/features/captive-portal


## License

GPL does not allow you to link GPL-licensed components with other proprietary software (unless you publish as GPL too).

GPL does not allow you to modify the GPL code and make the changes proprietary, so you cannot use GPL code in your non-GPL projects.

If you wish to integrate this software into your commercial software package, or you are a corporate entity with more than 10 employees, then you should obtain a per-instance license, or a site-wide license, from http://jpelectron.com/purchase.html

For all other use cases please consider: <a href='https://ko-fi.com/C0C54S4JF' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi2.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>


## Related repositories

https://github.com/JPElectron/DNSRedirOSSUpdater

https://github.com/JPElectron/keywords


## Version History

v7.2.0.18 r10/06/2020
 - Removed license file, client limits, and telemetry
 
Versions below this line are no longer supported (on any OS)

v7.2.0.14 r04/2019
- Fixed problem with SRV record types
- Improved EDNS compliance

v7.2.0.11 r08/2017
- Installer updates only, no exe change
- Includes the latest setup wizard
- Includes the latest keyword updater
- Includes the latest reporting options

v7.2.0.11 r03/2017
- Fixed problem with TXT, HINFO, ISDN and X25 record types
- Improved handling of changed file detection
- Improved setup steps in consideration of Windows 10 clients

v7.2.0.8 r02/2017
- Fixed error in DailyLogs
- Fixed problem with installer on Server 2016

v7.2.0.7 r08/2016
Note: New and/or different INI settings have been added, see the readme.
- Set case sensitivity used on keyword lists
    ...default is DNSCase=Insensitive
- Replace DNS replies containing known IPs to be NXDomain response instead
    ...useful to prevent malware/viruses, and on ISPs with guide/ad/search pages for typos
- Ability to define IPv4 and/or IPv6 addresses using CIDR notation
    ...in files AuthClientsFile= or NXDForceFile=
- Removed GUI since everyone's been using the service for years now
- Improved setup wizard, reporting and IIS files now included in package
- Improved buffering of log file output
- Easier license upgrade process
- Requires .NET Framework 4.6.1 - performance and security enhancements (Server 2003 support is gone)

v7.1.0.1 r03/2011
Note: New and/or different INI settings have been added, see the readme.
- Ability to define multiple IPv4 and/or IPv6 addresses (comma separated)
    ...define secondary, tertiary upstream DNS servers
    ...supports DNS load sharing (multiple IPs in the answer back to clients)
- Regular expressions (regex) can be used in blocked or allowed keyword lists
- Supports larger keyword lists, loads changes without restarting the software
- Faster DNS response time <1ms
- Improved performance (really muti-threaded, runs as 32-bit or 64-bit)
- Improved logging (goes into the \DailyLogs folder)
- Improved handling of different DNS record types (A, AAAA, MX, TXT, CNAME, SOA, NS, PTR, etc.)
- Added authonline.dnsredirctrl.com lookup, see FAQ 97
- Changed JoinType= valid options are Online/Auth/Both
- Added ResetClientFile= which when looked up sets client defaults (not authorized, not bypassing the block)
- Removed TimeRestriction options, see FAQ 128
- Ability to run as a native Windows service
- No dependency on VB6 runtimes or winsock
- Requires .NET Framework 4.x

v7.0.0.3 r12/2009

v7.0.0.2 r11/2009

v7.0.0.1 r11/2009 - requires .NET Framework 2.0 SP2

v6.4.09 r08/2009 - installer updates only, no exe change

v6.4.09 r02/2009

v6.4.08 r11/2008

v6.4.07 r08/2008 - installer updates only, no exe change

v6.4.07 r05/2008

v6.4.06 r03/2008

v6.4.05 r12/2007

v6.4.04 r11/2007

v6.4.02 r10/2007

v6.3.01 r06/2006

v6.3.00 r06/2006

v6.2.04 r03/2006

v6.2.03 r03/2006

v5.4.05 r08/2005

v5.1.45 r04/2005

v5.1.08 r01/2005

v4.2.25 r09/2004

v4.0.11 r07/2004

v3.0.12 r12/2003

v3.0.09 r11/2003

v2.6.00 r09/2003

v2.5.00 r08/2003

v2.4.00 r08/2003

v2.0.00 r06/2003

v1.x and v0.x(beta) began in Y2K (perhaps much earlier, but I didn't keep great records back then) released to clients and a select few closed beta participants

[End of Line]

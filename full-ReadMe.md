<font face="Verdana" size="2">DNS Redirector - Full Readme<br>
<br>
<font face="Verdana" style="font-size: 8.5pt"><br>
&nbsp;&nbsp;</font><b>System Requirements</b><br>
<br>
Supported Operating Systems...<br>
&nbsp; - Windows Server 2019<br>
&nbsp; - Windows Server 2016<br>
&nbsp; - Windows Server 2012 R2<br>
&nbsp; - Windows Server 2012<br>
&nbsp; - Windows Server 2008 R2 SP1<br>
<font face="Verdana" style="font-size: 5pt">&nbsp;<br>
</font>
&nbsp; X <font face="Verdana" style="font-size: 8.5pt">Windows 10, 8.1, 8, 7 SP1 - strongly discouraged due IIS limitations, 
but acceptable when pointing to another IIS server running on server OS</a>.</font><br>
&nbsp; X <font face="Verdana" style="font-size: 8.5pt">Windows XP/Vista or Server 2000/2003/2008 - cannot be used as the server, not supported.</font><br>
<br>
Minimum Hardware Requirements...<br>
&nbsp; 1 GHz or faster processor<br>
&nbsp; 4 GB of RAM<br>
&nbsp; 10 MB of available hard disk space (also 2.5 GB for <a href="full-FAQ.md#108">.NET Framework 4.6.1</a>)<br>
<br>
Networking Requirements...<br>
&nbsp; Server must have a static IP on the LAN (also static IP on the Internet/WAN or VPN infrastructure when used for Internet filtering external clients)<br>
&nbsp; Server should ideally have a wired Ethernet connection (not wireless)<br>
&nbsp; Installation on <a href="full-FAQ.md#91">existing domain controller</a> is acceptable.<br>
&nbsp; Separate server and LAN/VLAN/SSID is strongly suggested to mitigate security risks when used for public or open wireless networks.<br>
<br>
Any device with a TCP/IP connection is <a href="full-FAQ.md#98">considered a client</a> and can resolve DNS records through the DNS Redirector server.<br>
&nbsp; Windows, Apple/Mac, Linux/Unix, Playstation/Xbox, Tablet/eBook readers, Android devices, iPhone/iPad, etc.<br>
<br>
<font face="Verdana" style="font-size: 8.5pt"><br>
&nbsp;&nbsp;</font><b>Setup Instructions</b><br>
<br>
<font color="#006600"><b>Suggested method:</b></font> Scripted walkthrough...<br>
<font face="Verdana" style="font-size: 5pt">&nbsp;<br>
</font>
&nbsp;&nbsp;&nbsp;&nbsp; 1) Download the software from <a href="https://github.com/JPElectron/DNSRedirOSS/raw/main/dnsredir-download.zip">dnsredir-download.zip</a><br>
<font face="Verdana" style="font-size: 5pt">&nbsp;<br>
</font>
&nbsp;&nbsp;&nbsp;&nbsp; 2) Right-click on the .zip you downloaded, select Properties, click the Unblock button<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; (if this button is not present just proceed), click OK, then extract to C:\DNSREDIR<br>
<font face="Verdana" style="font-size: 5pt">&nbsp;<br>
</font>
&nbsp;&nbsp;&nbsp;&nbsp; 3) In the C:\DNSREDIR folder, right-click on _setup_.bat and select "Run as administrator"<br>
<font face="Verdana" style="font-size: 5pt">&nbsp;<br>
</font>
&nbsp;&nbsp;&nbsp;&nbsp; 4) Follow the prompts on-screen<br>
<font face="Verdana" style="font-size: 5pt">&nbsp;<br>
</font>

<br>
<br>
<b><a name="sec2">Implementation Considerations</a></b><br>
<br>
Use a standard LAN with a hardware firewall as the default gateway...<br>
See the <a href="https://drive.google.com/drive/folders/1KNDQZk0YMj6DSHv7fAJYGYBwAFhcyvd8">Network Examples</a>. Notice that <a href="full-FAQ.md#32">proxy</a>/SOCKS, ISA, or <a href="full-FAQ.md#33">ICS</a> is not compatible.<br>
<br>
When used for a public HotSpot...<br>
The guest LAN should be completely isolated from any internal/office LAN as shown in 
<a href="https://drive.google.com/drive/folders/1KNDQZk0YMj6DSHv7fAJYGYBwAFhcyvd8">Network Examples</a> 06, 09, 10<br>
You should mitigate problems as discussed in 
<a href="full-FAQ.md#34">FAQ 34</a>, 
<a href="full-FAQ.md#39">FAQ 39</a>, 
<a href="full-FAQ.md#113">FAQ 113</a>, 
<a href="full-FAQ.md#126">FAQ 126</a>.<br>
<br>
When used for content filtering...<br>
A dedicated server is not required; installation on your existing domain controller(s), small business server, or home server is adequate.<br>
<br>
DNS Redirector will try and bind DNS service to all IPs assigned to the server...<br>
If Microsoft's DNS service (found on some Windows Servers or Active Directory domain controllers) is installed see <a href="full-FAQ.md#91">FAQ 91</a>.<br>
If another DNS server or something using the same ports is installed see <a href="full-FAQ.md#4">FAQ 4</a>.<br>
<br>
You will need to change DHCP scope properties (option 6, DNS server)...<br>
The IP address used by DNS Redirector needs to be the only one handed out as the DNS server.<br>
If running multiple instances of DNS Redirector (only for content filtering, see <a href="full-FAQ.md#28">FAQ 28</a>) then add the IP of every DNS Redirector server.<br>
<br>
No NAT and no DNS separation...<br>
The DNS Redirector server and all clients cannot be separated by a NAT device, see <a href="full-FAQ.md#37">FAQ 37</a>, <a href="full-FAQ.md#142">FAQ 142</a>.<br>
Every client should use the IP of the DNS Redirector server as their default DNS server (usually provided via DHCP), another DNS server cannot exist in-between.<br>
<br>

<br>
For third-party software that is known to work with or aid in the use of DNS Redirector see <a href="full-FAQ.md#71">FAQ 71</a>.<br>

<br>
<br>
<b><a name="sec3">Installation</a></b><br>
<br>
<input type="checkbox" name="stepA" value="Done"> Download the software from <a href="https://github.com/JPElectron/DNSRedirOSS/raw/main/dnsredir-download.zip">dnsredir-download.zip</a><br>
<br>
&nbsp;&nbsp;&nbsp;&nbsp; If you are upgrading see <a href="full-FAQ.md#103">FAQ 103</a>.<br>
<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepB" value="Done"> Configure C:\DNSREDIR\dnsredir.ini (see <a href="full-ReadMe.md#sec4">INI Settings</a> section below)<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepC" value="Done"> Setup IIS or other web server software (see <a href="full-ReadMe.md#sec5">Hosted Pages</a> section below)<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepD" value="Done"> Verify firewall exceptions have been defined, see <a href="full-FAQ.md#102">FAQ 102</a>.<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepE" value="Done"> Verify the working directory has adequate permissions, see <a href="full-FAQ.md#129">FAQ 129</a>.<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepF" value="Done"> For Captive Portal, whitelist domains you may need, see <a href="full-FAQ.md#121">FAQ 121</a>, <a href="full-FAQ.md#159">FAQ 159</a>.<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepG" value="Done"> For Internet filtering, whitelist domains you may need, see <a href="full-FAQ.md#112">FAQ 112</a>.<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepG" value="Done"> For Internet filtering, blacklist any domains you don't need, see <a href="full-FAQ.md#52">FAQ 52</a>, <a href="full-FAQ.md#106">FAQ 106</a>.<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepH" value="Done"> Start the DNS Redirector service<br><font face="Verdana" style="font-size: 5pt">
<br></font>
<input type="checkbox" name="stepI" value="Done"> Change your DHCP scope to hand out the DNS Redirector server IP as the only DNS server.<br>
&nbsp;&nbsp;&nbsp;&nbsp; <font face="Verdana" style="font-size: 8.5pt">(DHCP option 6, DNS server) This should be the same IP you specified for ListenOnIP= in dnsredir.ini</font><br>

<br>
<br>
<b><a name="sec4">INI Settings</a></b><br>
<br>
<font color="#008000"><b>Default</b></font> values are in green<br>
<font color="#0000FF"><b>Example</b></font> values are in blue<br>
<br>
All files referenced in the .ini are assumed to be in the C:\DNSREDIR working directory.<br>
All IP address fields will also accept an IPv6 address.<br>
<br>
<b><font color="#666666">Logging=</font><font color="#008000">Normal</font></b><br>
&nbsp; Sets the log file detail. A new log file is created each day within the DailyLogs folder, the filename is the date.<br>
Valid options are:<br>
<font color="#0000FF">Off</font> - No log is created (this is fastest and recommended for large networks)<br>
<font color="#0000FF">Normal</font> - Only queries modified/answered by DNS Redirector are logged<br>
<font color="#0000FF">Full</font> - Every query, response, and function is logged (useful for diagnostic/troubleshooting, use sparingly as log files become large quickly)<br>
<br>
<b><font color="#666666">Optimize=</font><font color="#008000">Speed</font></b><br>
&nbsp; Sets the string matching algorithm used on keyword lists.<br>
Valid options are:<br>
<font color="#0000FF">Speed</font> - this is fastest and recommended for large networks<br>
<font color="#0000FF">Memory</font> - this will use less memory (ideal for machines with low resources serving smaller networks)<br>
<br>
<b><font color="#666666">DNSCase=</font><font color="#008000">Insensitive</font></b><br>
&nbsp; Sets the case sensitivity used on keyword lists.<br>
Valid options are:<br>
<font color="#0000FF">Insensitive</font> - this is recommended for everyone<br>
<font color="#0000FF">Sensitive</font> - for special use/high-security networks<br>
<br>
<b><font color="#666666">ListenOnIP=</font><font color="#0000FF">192.168.0.2, 192.168.0.3</font></b><br>
&nbsp; Specify the static IP address(es) of this DNS Redirector server (recommended), see <a href="full-FAQ.md#4">FAQ 4</a>, <a href="full-FAQ.md#91">FAQ 91</a>.<br>
Or leave blank to bind on all system IPs (including the IPv4 loopback address 127.0.0.1)<br>
<br>
<b><font color="#666666">SimpleDNS=</font><font color="#0000FF">simpledns.txt</font></b><br>
&nbsp; File containing DNS A records that you want to resolve locally.<br>
The contents of the file needs to be in the following format:<br>
IP address[tab]Fully qualified domain name<br>
As shown in this example:<br>
192.168.0.1[tab]router.example.com<br>
192.168.0.2[tab]blocked.example.com<br>
192.168.0.3[tab]welcome.example.com<br>
&nbsp; Or as a catch-all:<br>
192.168.0.8[tab]*<br>
&nbsp; When using an asterisk all domain names will resolve to a single IP, regardless of being real or not.&nbsp; 
This method does not require a real DNS server to be specified under DNSServerIP= but will render all RedirectIP= and BlockedIP= functions disabled.&nbsp; 
This method is for specific scenarios where a real DNS server is not available (no Internet connection) and/or you need to make only a few internal sites available.&nbsp; 
Use the same steps as if you were setting up a RedirectIP= site at this IP, see the <a href="full-ReadMe.md#sec5">Hosted Pages</a> section.&nbsp; <br>
<br>
<b><font color="#666666">DNSServerIP=</font><font color="#0000FF">8.8.4.4, 8.8.8.8</font></b><br>
&nbsp; Specify the IP of a real DNS server. <br>
This is the DNS server that all normal queries are forwarded onto.&nbsp; 
On a corporate network you will usually declare the IP of your internal DNS or Active Directory 
integrated DNS server, otherwise declare the DNS server provided by your upstream Internet provider.&nbsp; 
This setting is always required, unless using SimpleDNS= with a catch-all asterisk record.&nbsp; <br>
<br>
<b><font color="#666666">RedirectIP=</font><font color="#0000FF">192.168.0.3</font></b><br>
&nbsp; Initially redirect clients to this IP, where your <a href="https://drive.google.com/drive/folders/1OReNRDrrkdnf8gjWTNRUZlYr3haM-w0i">welcome page</a> is hosted.<br>
When specified, the first time a client tries to browse the Internet they will 
be shown the website hosted at this IP address instead.&nbsp; 
When specifying RedirectIP= then AuthKeywordsFile= is also required.&nbsp; 
If initial redirection is not going to be used leave both settings blank.&nbsp; 
See the <a href="full-ReadMe.md#sec5">Hosted Pages</a> section for setting up a page at this IP address.&nbsp; 
This must be an IP address, not a URL, for information on redirecting to an existing website or 
URL see <a href="full-FAQ.md#30">FAQ 30</a>.&nbsp; <br>
<br></font><table border="0" id="RedirectIP" cellspacing="0" cellpadding="0">
	<tr>
		<td width="15">&nbsp;</td>
		<td><font face="Verdana" size="2">
		<b><font color="#666666">AuthKeywordsFile=</font><font color="#0000FF">authorized.txt</font></b><br>
		&nbsp; File containing keywords of domain names that, after resolved, authorizes the client to surf past the welcome page.<br>
		The contents of the file needs to include one or several complex/unique domain 
names to be treated as the "key" that allows users to browse past the RedirectIP= page.&nbsp; 
These do not have to be actual domain names registered on the Internet, you can make them up.&nbsp; 
Use SimpleDNS= so a made up domain name resolves to an IP.&nbsp; 
When a client does a DNS lookup for a matching domain name the client will be marked as Authorized.<br>
&nbsp; The system should work like this...&nbsp; (adapt it to your needs; payment page, password, registration, etc.)<br>
A) user joins the network, 
B) user gets DHCP lease including DNS Redirector as the DNS server, 
C) user starts browser and sees your terms and conditions page, 
D) user clicks a link to accept the agreement, 
E) user gets forwarded to another page that says "Welcome to the Internet" and includes a clear image referenced at http://surfon.dnsredirctrl.com/clear.gif, 
F) the browser does a DNS lookup for surfon.dnsredirctrl.com 
G) DNS Redirector finds that surfon.dnsredirctrl.com matches the domain name specified in the AuthKeywordsFile, 
H) user can now browse the Internet freely.&nbsp; <br>
		<br>
		<b><font color="#666666">AlwaysKeywordsFile=</font><font color="#0000FF">always.txt</font></b><br>
		&nbsp; File containing keywords of domain names that clients are always allowed to visit, even if they have not been authorized.<br>
		In a paid HotSpot scenario you would add the domain name(s) of your payment processor to this file 
so users can visit the site in order to pay for access and then become authorized.&nbsp; 
See <a href="full-FAQ.md#159">FAQ 159</a>.&nbsp; 
Leave this setting blank if you are not going to use it.&nbsp; <br>
		<br>
		<font color="#666666"><b>AuthClientsFile=</font><font color="#0000FF">authclients.txt</font></b><br>
		&nbsp; File containing IPs of local network clients that are always allowed to surf, even if they have not been authorized.<br>
		Useful for static-IP machines on the same LAN as the hotspot that shouldn't have to pay or become 
authorized to surf; such as a kiosk, the IT manager, back office, or receptionist's computer.&nbsp; 
Leave this setting blank if you are not going to use it.&nbsp; 
Note: This function is for special circumstances only, in most cases the public/hotspot network should be 
completely separate from the internal/office network as shown in <a href="https://drive.google.com/drive/folders/1KNDQZk0YMj6DSHv7fAJYGYBwAFhcyvd8">Network Examples</a>.&nbsp; <br>
		<br></font></td>
	</tr>
</table>

<font face="Verdana" size="2">
<b><font color="#666666">BlockedIP=</font><font color="#0000FF">192.168.0.2</font></b><br>
&nbsp; Domain names matched in the BlockedKeywordsFile= below will resolve to this IP, where your <a href="https://drive.google.com/drive/folders/1OReNRDrrkdnf8gjWTNRUZlYr3haM-w0i">blocked page</a> is hosted.<br>
If content filtering is not going to be used leave this setting blank.&nbsp; 
This must be an IP address, not a URL.&nbsp; 
When specifying BlockedIP= then BlockedKeywordsFile= is also required.&nbsp; 
See the <a href="full-ReadMe.md#sec5">Hosted Pages</a> section for setting up a page at this IP address.&nbsp; <br>
<br></font><table border="0" id="BlockedIP" cellspacing="0" cellpadding="0">
	<tr>
		<td width="15">&nbsp;</td>
		<td><font face="Verdana" size="2">
		<b><font color="#666666">BlockResponse=</font><font color="#008000">Lookup</font></b><br>
		&nbsp; Valid options are:<br>
		<font color="#0000FF">Lookup</font> - resolves to the BlockedIP only if the domain name is real (does a lookup at the DNSServerIP= first)<br>
		<font color="#0000FF">Fast</font> - resolves to the BlockedIP even if the domain name does not exist<br>
		<br>
		<b><font color="#666666">BlockedKeywordsFile=</font><font color="#0000FF">blocked.txt</font></b><br>
		&nbsp; File containing keywords of domain names that clients cannot visit.<br>
		To automate the updating of keywords see <a href="full-FAQ.md#52">FAQ 52</a>.&nbsp; 
To block everything see <a href="full-FAQ.md#5">FAQ 5</a>.&nbsp; 
If blocking is not going to be used leave this setting blank.&nbsp; 
When specifying BlockedKeywordsFile= you must also specify BlockedIP= and host a website at that IP or web surfing will be slow.&nbsp; <br>
		<br>
		<b><font color="#666666">AllowedKeywordsFile=</font><font color="#0000FF">allowed.txt</font></b><br>
		&nbsp; File containing keywords of domain names that clients are allowed to visit.<br>
		Sometimes good blocking keywords can prevent clients from reaching legitimate content, this list corrects that.&nbsp; 
See <a href="full-FAQ.md#112">FAQ 112</a>.&nbsp; 
If blocking is not going to be used leave this setting blank.&nbsp; <br>
		<br>
		<b><font color="#666666">BypassBlockFile=</font><font color="#0000FF">bypassblock.txt</font></b><br>
		&nbsp; File containing keywords of domain names that, after resolved, allows the client to view blocked content.<br>
		The contents of the file needs to include one or several complex/unique domain 
names to be treated as the "key" that allows users to browse past the BlockedIP= page.&nbsp; 
These do not have to be actual domain names registered on the Internet, you can make them up.&nbsp; 
Use SimpleDNS= so a made up domain name resolves to an IP.&nbsp; 
Note that after blocking is off you will need to close and open any browser windows, this is necessary 
to clear the browser's DNS cache for websites visited prior, otherwise those sites may still be blocked.&nbsp; 
Restarting DNS Redirector will turn blocking back on for all clients.&nbsp; 
By implementing ResetClientFile= you can turn blocking back on per-client.&nbsp; 
Note that a client who visits a BypassBlockFile= domain name before a AuthKeywordsFile= domain name 
will be able to browse freely, but will not be authorized.&nbsp; 
If blocking is not going to be used leave this setting blank.&nbsp; <br>
		<br></font></td>
	</tr>
</table>

<font face="Verdana" size="2">
<b><font color="#666666">NXDForceFile=</font><font color="#008000">nxdforce.txt</font></b><br>
&nbsp; File containing IPs that when found in any DNS reply will be replaced with NXDomain response instead.<br>
This is useful to undo NXDomain hijacking (as some ISPs like to do) and for additional protection against badware, malware, scumware.<br>
<br>
<b><font color="#666666">ResetClientFile=</font><font color="#008000">resetclient.txt</font></b><br>
&nbsp; File containing keywords of domain names that, after resolved, causes DNS Redirector to forget the client.&nbsp; 
This removes the client from the online clients list; de-authorizes the client, re-enables the block, and executes the LeaveAction if set.<br>
<br>
<b><font color="#666666">ActionNumber=</font><font color="#008000">0</font></b><br>
&nbsp; Perform the JoinAction specified below; <font color="#0000FF">1</font> means every time, 
<font color="#0000FF">2</font> means for every 2nd client who joins, 
<font color="#0000FF">3</font> for every 3rd client who joins, etc.<br>
If actions are not going to be used leave this set to <font color="#0000FF">0</font>.<br>
<br>
<b><font color="#666666">JoinType=</font><font color="#008000">Online</font></b><br>
&nbsp; Valid options are:<br>
<font color="#0000FF">Online</font> - executes JoinAction for any client that starts resolving through DNS Redirector<br>
<font color="#0000FF">Auth</font> - executes JoinAction only when a client becomes authorized<br>
<font color="#0000FF">Both</font> - executes JoinAction when a new client starts resolving through DNS Redirector, and again when that client becomes authorized<br>
Only client's who authorize themselves trigger the action, clients specified in the AuthClientsFile= or clients manually marked as Authorized in the GUI will not trigger the JoinAction.<br>
&nbsp;</font><table border="0" id="ActionNumber" cellspacing="0" cellpadding="0">
	<tr>
		<td width="15">&nbsp;</td>
		<td><font face="Verdana" size="2">
		<font color="#666666"><b>JoinAction=</b></font><br>
		&nbsp; File you want to launch or execute when a client joins the network. 
		This could be a .exe, .wav, .bat or other script. If a join action is not desired then leave this blank.&nbsp; 
		The client's IP is passed as a variable after the command for use with a third-party script or application, see <a href="full-FAQ.md#62">FAQ 62</a>.&nbsp; 
		Specify the full path to the file, for example C:\DNSREDIR\join.bat<br>
		<br>
		<font color="#666666"><b>LeaveAction=</b></font><br>
		&nbsp; File you want to launch or execute when a client leaves the network, used only when ActionNumber=1.&nbsp;  
		This could be a .exe, .wav, .bat or other script. If a leave action is not desired then leave this blank.&nbsp; 
		The client's IP is passed as a variable after the command for use with a third-party script or application.&nbsp; 
		Specify the full path to the file, for example C:\DNSREDIR\leave.bat<br>
		<br></font></td>
	</tr>
</table>

<font face="Verdana" size="2">
<b><font color="#666666">ClientTimeout=</font><font color="#008000">20</font></b><br>
&nbsp; Interval in minutes before an active client is considered gone or left the network, based on the last DNS query received.&nbsp; 
This removes the client from the online clients list; depending on the features enabled it de-authorizes the client, re-enables the block, and executes the LeaveAction if set.<br>
<br>
The following INI settings are depreciated in v7.2.x.x<br>
GetClientName=<br>
MinToTray=<br>
CloseToTray=<br>

<br>
<br>
<b><a name="sec5">Hosted Pages</a></b><br>
<br>
Using IIS on the same server as DNS Redirector to host the welcome and/or blocked pages is suggested.&nbsp; 
Optionally, you can declare the IP of another web server that is internal or external to the DNS Redirector network.&nbsp; 
IIS on a non-server OS has restrictions</a>, 
such configuration is not supported or recommended.&nbsp; 
Using <a target="_blank" href="https://github.com/JPElectron/SimpleHTTP">SimpleHTTP</a> 
or <a target="_blank" href="http://httpd.apache.org/download.cgi">Apache HTTP Server</a> may be appropriate in some cases.<br>
<br>
Depending on the features enabled in DNS Redirector you may need multiple sites, each requiring its own IP address.&nbsp; 
Add multiple IP addresses to the same NIC under the Advanced button in TCP/IP properties.<br>
<br>
<input type="checkbox" name="step00" value="Done"> verify that "ASP" and "Server Side Includes" are installed with IIS&nbsp; 
(see screenshot for <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS7-ASPinstall.gif</a> 
or <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS8-ASPinstall.gif</a>)<br>
<br>
If <b><font color="#666666">RedirectIP=</font><font color="#0000FF">192.168.0.3</font></b> complete the following steps...<br>
<input type="checkbox" name="step01" value="Done"> create a folder for the site root, such as C:\Inetpub\welcome<br>
&nbsp;-&nbsp; in IIS Manager create a site:&nbsp; 
(see details for <a target="_blank" href="http://technet.microsoft.com/en-us/library/cc772350(WS.10).aspx">IIS7</a>)<br>
<input type="checkbox" name="step03" value="Done"> running at 192.168.0.3 | port 80 | no Host header | path set as the folder created above<br>
&nbsp;-&nbsp; for IIS6: leave checked "Allow anonymous access to this Web site" | leave checked "Read" | check "Run scripts (such as ASP)"<br>
<input type="checkbox" name="step02" value="Done"> extract a <a href="https://drive.google.com/drive/folders/1OReNRDrrkdnf8gjWTNRUZlYr3haM-w0i">sample welcome page</a> to the folder created above<br>
<br>
If <b><font color="#666666">BlockedIP=</font><font color="#0000FF">192.168.0.2</font></b> complete the following steps...<br>
<input type="checkbox" name="step08" value="Done"> create a folder for the site root, such as C:\Inetpub\blocked<br>
&nbsp;-&nbsp; in IIS Manager create a site:&nbsp; 
(see details for <a target="_blank" href="http://technet.microsoft.com/en-us/library/cc772350(WS.10).aspx">IIS7</a>)<br>
<input type="checkbox" name="step10" value="Done"> running at 192.168.0.2 | port 80 | no Host header | path set as the folder created above<br>
<input type="checkbox" name="step09" value="Done"> extract a <a href="https://drive.google.com/drive/folders/1OReNRDrrkdnf8gjWTNRUZlYr3haM-w0i">sample blocked page</a> to the folder created above<br>
<input type="checkbox" name="step15" value="Done"> download: <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">REG-UrlSegmentMaxLength.zip</a> then open the .reg file<br>
&nbsp;&nbsp;&nbsp;&nbsp; <font face="Verdana" style="font-size: 8.5pt">this is necessary so certain blocked content is replaced correctly, see <a href="full-FAQ.md#169">FAQ 169</a></font><br>
<br>
<a name="sec6">for every site created above...</a><br>
<br>
<input type="checkbox" name="step23" value="Done"> add the HTTP Header: "Cache-Control: no-store, no-cache, post-check=0, pre-check=0"&nbsp; 
(see screenshot for <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS6-HttpHeadersTab.gif</a>
or <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS7-HttpHeadersTab.gif</a>)<br>
&nbsp;&nbsp;&nbsp;&nbsp; <font face="Verdana" style="font-size: 8.5pt">META tags which preventing caching (as included in the <a href="https://drive.google.com/drive/folders/1OReNRDrrkdnf8gjWTNRUZlYr3haM-w0i">example pages</a>) are required in addition to this HTTP Header 
(see <a target=_blank href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9">rfc2616-sec14.9</a> 
and <a target=_blank href="http://msdn.microsoft.com/en-us/library/ms533020(VS.85).aspx#Use_Cache-Control_Extensions">msdn</a>)</font><br>
<br>
<input type="checkbox" name="step24" value="Done"> on IIS6 when ASP.NET is installed ensure the version is set to 2.x or later&nbsp; 
(see <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS6-ASPdotNETver.gif</a>)<br>
<br>
<input type="checkbox" name="step25" value="Done"> on IIS7 under Error Pages, Edit Feature Settings, set "Custom error pages"&nbsp; 
(see <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS7-ErrorPagesSettings.gif</a>)<br>
<br>
<input type="checkbox" name="step26" value="Done"> Enable Parent Paths&nbsp; 
(see screenshot for <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS6-ASPEnableParentPaths.gif</a>
or <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS7-ASPEnableParentPaths.gif</a>)<br>
<br>
<input type="checkbox" name="step27" value="Done"> check NTFS permissions on the root folder&nbsp; 
(see screenshot for <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS6-DirSecurity.gif</a> 
or <a href="https://drive.google.com/drive/folders/1lMxuJpa2z43nnDBOQoBmeGVOdbOvGOcx">IIS7-DirSecurity.gif</a>)<br>
&nbsp;&nbsp;&nbsp;&nbsp; <font face="Verdana" style="font-size: 8.5pt">(see <a target="_blank" href="http://support.microsoft.com/kb/812614">kb812614</a> / <a target="_blank" href="http://support.microsoft.com/kb/981949">kb981949</a>)</font><br>
<br>
<input type="checkbox" name="step28" value="Done"> verify the site is running, type: http://[IP from above] in a browser on this server and on a client computer<br>

<br>
<br>
[End of Line]

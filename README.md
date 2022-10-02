# MarcBot
A chat UI with console app backend. 


# Apache reverse proxy config for web sockets:

run
`sudo a2enmod rewrite && sudo service apache2 restart`

Follow this [guide](https://www.serverlab.ca/tutorials/linux/web-servers-linux/how-to-reverse-proxy-websockets-with-apache-2-4/), but use the modify it as below: 

```
<VirtualHost *:8080>
  ServerName yourdomain.org.uk

  RewriteEngine on
  RewriteCond ${HTTP:Upgrade} websocket [NC]
  RewriteCond ${HTTP:Connection} upgrade [NC]
  RewriteRule .* "ws://localhost:8001/$1" [P,L]

  ProxyPass / ws://localhost:8001/
  ProxyPassReverse / ws://localhost:8001/
  ProxyRequests off
</VirtualHost>
```

Run  `sudo nano /etc/apache2/ports.conf` and add port 8080

and then restart apache

`sudo systemctl restart apache2`
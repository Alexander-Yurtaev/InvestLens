echo Generate certificate for localhost

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\localhost.key" -out "C:\docker-certs\invest-lens\localhost.crt" -subj "/CN=localhost" -addext "subjectAltName=DNS:localhost"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\localhost.pfx" -inkey "C:\docker-certs\invest-lens\localhost.key" -in "C:\docker-certs\invest-lens\localhost.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_gateway

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_gateway.key" -out "C:\docker-certs\invest-lens\investlens_gateway.crt" -subj "/CN=investlens.gateway" -addext "subjectAltName=DNS:investlens.gateway"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_gateway.pfx" -inkey "C:\docker-certs\invest-lens\investlens_gateway.key" -in "C:\docker-certs\invest-lens\investlens_gateway.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_web

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_web.key" -out "C:\docker-certs\invest-lens\investlens_web.crt" -subj "/CN=investlens.web" -addext "subjectAltName=DNS:investlens.web"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_web.pfx" -inkey "C:\docker-certs\invest-lens\investlens_web.key" -in "C:\docker-certs\invest-lens\investlens_web.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_worker

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_worker.key" -out "C:\docker-certs\invest-lens\investlens_worker.crt" -subj "/CN=investlens.worker" -addext "subjectAltName=DNS:investlens.worker"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_worker.pfx" -inkey "C:\docker-certs\invest-lens\investlens_worker.key" -in "C:\docker-certs\invest-lens\investlens_worker.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_data_api

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_data_api.key" -out "C:\docker-certs\invest-lens\investlens_data_api.crt" -subj "/CN=investlens.data.api" -addext "subjectAltName=DNS:investlens.data.api"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_data_api.pfx" -inkey "C:\docker-certs\invest-lens\investlens_data_api.key" -in "C:\docker-certs\invest-lens\investlens_data_api.crt" -passout pass:YourPassword123


pause
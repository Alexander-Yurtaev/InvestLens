echo Generate certificate for localhost

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\localhost.key" -out "C:\docker-certs\invest-lens\localhost.crt" -subj "/CN=localhost" -addext "subjectAltName=DNS:localhost"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\localhost.pfx" -inkey "C:\docker-certs\invest-lens\localhost.key" -in "C:\docker-certs\invest-lens\localhost.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_gateway

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_gateway.key" -out "C:\docker-certs\invest-lens\investlens_gateway.crt" -subj "/CN=investlens_gateway" -addext "subjectAltName=DNS:investlens_gateway"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_gateway.pfx" -inkey "C:\docker-certs\invest-lens\investlens_gateway.key" -in "C:\docker-certs\invest-lens\investlens_gateway.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_web

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_web.key" -out "C:\docker-certs\invest-lens\investlens_web.crt" -subj "/CN=investlens_web" -addext "subjectAltName=DNS:investlens_web"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_web.pfx" -inkey "C:\docker-certs\invest-lens\investlens_web.key" -in "C:\docker-certs\invest-lens\investlens_web.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_worker

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_worker.key" -out "C:\docker-certs\invest-lens\investlens_worker.crt" -subj "/CN=investlens_worker" -addext "subjectAltName=DNS:investlens_worker"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_worker.pfx" -inkey "C:\docker-certs\invest-lens\investlens_worker.key" -in "C:\docker-certs\invest-lens\investlens_worker.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_data_api

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_data_api.key" -out "C:\docker-certs\invest-lens\investlens_data_api.crt" -subj "/CN=investlens_data_api" -addext "subjectAltName=DNS:investlens_data_api"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_data_api.pfx" -inkey "C:\docker-certs\invest-lens\investlens_data_api.key" -in "C:\docker-certs\invest-lens\investlens_data_api.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_auth_api

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_auth_api.key" -out "C:\docker-certs\invest-lens\investlens_auth_api.crt" -subj "/CN=investlens_auth_api" -addext "subjectAltName=DNS:investlens_auth_api"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_auth_api.pfx" -inkey "C:\docker-certs\invest-lens\investlens_auth_api.key" -in "C:\docker-certs\invest-lens\investlens_auth_api.crt" -passout pass:YourPassword123


echo Generate certificate for investlens_grafana

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\invest-lens\investlens_grafana.key" -out "C:\docker-certs\invest-lens\investlens_grafana.crt" -subj "/CN=investlens_grafana" -addext "subjectAltName=DNS:investlens_grafana"

openssl pkcs12 -export -out "C:\docker-certs\invest-lens\investlens_grafana.pfx" -inkey "C:\docker-certs\invest-lens\investlens_grafana.key" -in "C:\docker-certs\invest-lens\investlens_grafana.crt" -passout pass:YourPassword123


pause
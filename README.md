# ReverseProxy
ReverseProxy for Kubernetes



## Docker setup:

Install Docker for Windows
Switch to Windows containers

> ReverseProxy\Docker> docker build --pull -t reverseproxy:test1 -f ReverseProxy.dockerfile ..\

> ReverseProxy\Docker> docker run --rm -it -p 8000:80 reverseproxy:test1

# ReverseProxy
Kubernetes Ingress-Controller using Kestrel based ReverseProxy.



## Docker setup:

1. Install Docker Desktop for Windows (https://docs.docker.com/docker-for-windows/install/#install-docker-desktop-on-windows)

2. Switch to Windows containers (Right click "Docker Whale" in System tray)

3. Build docker image "reverseproxy:test1" for ReverseProxy
    > ReverseProxy\Docker> docker build --pull -t reverseproxy:test1 -f ReverseProxy.dockerfile ..\

4. Run the docker image locally and point the traffic on "8000" to the doker container
    > ReverseProxy\Docker> docker run --rm -it -p 8000:80 reverseproxy:test1

5. Test the webservice default path:

http://localhost:8000/weatherforecast1



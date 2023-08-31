# NetChallenge10 소켓 서버 도커 설정 및 소켓 구동 방법

1. 서버 접속

2. sudo iptables -I INPUT 1 -p tcp --dport 4200 -j ACCEPT

3. sudo iptables -I OUTPUT 1 -p tcp --dport 4200 -j ACCEPT

4. docker build -t devox_socket -f DockerFile .

5. docker run -it -p 4200:4200 devox_socket /bin/bash

6. cd socketServer/

7. dotnet GameServer.dll --uniqueID 1 --name GameServer --port 4200 --maxConnectionNumber 1000 --maxRequestLength 65000 --receiveBufferSize 65000 --sendBufferSize 65000 --roomMaxCount 50 --roomMaxUserCount 1000 --roomStartNumber 0


# 7번 완료 후, 콘솔창에 "이후 서버 초기화 성공" 메세지가 출력되면 성공입니다.

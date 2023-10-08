FROM python:3.9
#Docker 이미지 내부에서 RUN, CMD, ENTRYPOINT의 명령이 실행될 디렉터리를 설정합니다.
WORKDIR /home

COPY  . /home/



RUN bash setup.sh

# 쉘을 사용하지 않고 컨테이너가 시작되었을 때, Inference.py 실행
CMD python /home/Inference.py
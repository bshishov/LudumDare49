FROM python:3.9

WORKDIR /opt/app
COPY . /opt/app

RUN pip install --trusted-host pypi.python.org -r requirements.txt

EXPOSE 6789

ENV PYTHONPATH=/opt/app:/opt/app/server

CMD ["python", "-m", "server", "--host", "0.0.0.0", "--port", "6789"]

FROM ubuntu
RUN mkdir /app
COPY . /app/
WORKDIR /app
RUN chmod +x startgeth.sh
RUN chmod +x geth
CMD ["sh", "./startgeth.sh"]
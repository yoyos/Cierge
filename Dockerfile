FROM microsoft/dotnet:latest
WORKDIR ./app

COPY Cierge .

#COPY demo_rsa_signing_key_json /run/secrets/

# BUILD LINES
ENV ASPNETCORE_ENVIRONMENT Production


#COPY ./entrypoint.sh .
#RUN sed -i.bak 's/\r$//' ./entrypoint.sh
#RUN chmod +x ./entrypoint.sh
#CMD /bin/bash ./entrypoint.sh

CMD "dotnet run --server.urls=https://localhost:$PORT --no-launch-profile"
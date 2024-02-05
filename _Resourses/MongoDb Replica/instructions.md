# Convert MongoDb standalone server to Replication

Video tutorial
https://youtu.be/sw-J5ytvYe8?si=3kJxBW6oDcaETm6N

My Working Solution: 
https://stackoverflow.com/a/77932054/3944285


1. Set replica name using command bellow: (Mine is called "rs0").
```bash
    echo -e "replication:\n  replSetName: \"rs0\"" | sudo tee -a /etc/mongod.conf
```
2. 
```bash
    Restart mongod service
```
3. 
```bash
    sudo systemctl restart mongod
```
4. Make sure the service/server is running.
```bash
    sudo systemctl status mongod
```
5. Enter mongosh (MongoDb Shell must be installed) 
```bash
    mongosh
```
6. Initialize your members. First one is Primary
```bash
    rs.initiate({ _id: 'rs0', members: [ { _id: 0, host: 'localhost:27017' } ]})
```
7. Check the status using  
```bash
    rs.status()
```
8. Run this command if you want the service to run at Linux startup.
```bash
    sudo systemctl enable mongod
```

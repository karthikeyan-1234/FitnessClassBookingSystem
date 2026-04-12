## Accounts API

### To build and run the Accounts API in docker

```powershell
PS C:\Users\mail2\source\repos\FitnessClassBookingSystem\src> docker build -t fcbs-accounts:stable -f Accounts/Presentation/Dockerfile .
```

### The above command will add an image of fcbs-account with tag stable to the docker images list. Run the below command
### to run a container instance

```powershell
PS C:\Users\mail2> docker run -d `
   -p 8080:8080 `
   -p 8081:8081 `
   --name fcbs-accounts-api `
   fcbs-accounts:stable
```

### Followed by this, the Accounts api will be accessible as below
http://localhost:8080/swagger/index.html - Accounts
# protobuf-perf
Performance Investigations

## Results from 24. June

### 1th try
![image](https://user-images.githubusercontent.com/10084630/175491542-88e7c2dd-d857-458f-897a-d21bdfc765c2.png)

![image](https://user-images.githubusercontent.com/10084630/175491627-cd8611dc-c6e7-49f1-997c-d3d9e0dbebe1.png)

It looks growing now (ThreadLocal RuntimeTypeModel, but with all the locks inside)
![image](https://user-images.githubusercontent.com/10084630/175491885-e520d931-64a7-4381-bfdc-0963ddb7a2fe.png)

### 2nd try
The same test with 4 Mio objects:

![image](https://user-images.githubusercontent.com/10084630/175501241-2a7985a1-2719-49c9-9116-80fb9a264205.png)

![image](https://user-images.githubusercontent.com/10084630/175502448-9b938b93-7ea9-4787-b649-1a5984d42e62.png)

![image](https://user-images.githubusercontent.com/10084630/175503446-6823576c-fe5f-4129-9bc5-ae9bfed0c882.png)

The red area is the overprovisioning with Degree=20, this is ok!
Basically the overall CPU usage is growing from 10% to 20% (8% to 22% if we like to read ist very optimistic) between degree=1 and degree=16

### Interesting!!

Modified the runtimeModel: it returns the provided InitValue withoud using real "deserializer" with the following results on my laptop (Processorcount=8, Hyperthreads and Turbo Booster)

![image](https://user-images.githubusercontent.com/10084630/175536394-a9ff208f-0c34-49e6-a8cd-c6e603cce7a5.png)

Speed is growing until 8 cores

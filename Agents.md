# AI agents guidelines 
welcome to this project .All AI agents ,coding assistants and automated tools must adhere to the project's specific coneventions and instructions 

### Unity Specific Guidelines 
- Do not hand-author or edit `.meta` files.
- Follow Unity-First workflows for gameplay,scene,prefabs, assets and C# scripts.

### Input System Guidelines 
- Always use `Input System Package(New)`
-Do not introduce or relay on legacy Input Manager(`UnityEngine.Input` old system) for new work.
- If touching Input related code ,align it with the New Input System patterns already used in the project.

### Verification Before Done
- Never mark a task complete without proving it works.
- Diff behaviour between main and your changes when relevent.
- Run test ,check logs, demonstrate correctness 

# Wordle Client Server

## To build WordleClient:
1. Add project to Unity Hub
    1. In Projects tab, click the "Add" button next to "New Project"
    2. Select Add project from disk
    3. Select "WordleClient" folder
    4. Make sure Editor Version is Unity 6.2 (6000.2.12f1) to avoid any issues
2. Open the project by clicking on it
3. Build the project
    1. Once everything is loaded, click on "File" from the menu bar above
    2. Click on Build Profiles
    3. Click on Scene List from the list on the left side
    4. Check that all the scenes are in the list (StartScene, LobbyScene, GameScene)
        1. If any scenes are missing, find the project pane (usually the big horizontal pane below with a tab next to console)
        2. Navigate to Assets/_Project/Scenes
        3. Drag and drop all the missing scenes to the scene list.
    5. Select a build platform
        1. If there are no available platforms, click on the desired platform
        2. Click "Install with Unity Hub"
        3. Unity Hub will pop up and auto-select the required module
        4. Click Install
        5. Unity Editor may require a restart if platform is still not available
    6. Click on build

## To build WordleServer:
1. Add project to Visual Studio 2022
    1. After opening Visual Studio 2022, select "Open a project or solution"
    2. Select "WordleServer.sln" in the WordleServer folder (Make sure all the .cs files and obj folder are in the same directory as WordleServer.sln)
2. Click on "Build" in the menu bar above
3. Click "Build Solution"
4. Navigate to the new "WordleServer/bin/Debug/net8.0/"
5. Open the executable file

I'm not sure how to build the server for Mac/Linux systems.
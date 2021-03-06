var io = require('socket.io')(process.env.PORT || 52300);

//Custom Classes
var Player = require('./Classes/Player.js');
var TakeDamage = require('./Classes/TakeDamage.js');
var Move = require('./Classes/Move.js');
var CollisionMove = require('./Classes/CollisionMove.js');
var Aim = require('./Classes/Aim.js');
var SeatMove = require('./Classes/SeatMove.js');
var Vector2 = require('./Classes/Vector2.js');

var players = [];
var sockets = [];
//var spawnPoints = [[0, 0], [-15, 5], [8, 11], [12, -7], [-16, -6], [0, -12]];
var spawnPoints = [[0, 0]];

console.log('Server has started');

io.on('connection', function(socket) {
    console.log('Connection Made!');

    var player = new Player();

    var thisPlayerID = player.id;

    players[thisPlayerID] = player;
    sockets[thisPlayerID] = socket;

    //Random Spawn Point for Player
    //var num = Math.floor((Math.random() * 6));
    var num = 0;
    player.position.x = spawnPoints[num][0];
    player.position.y = spawnPoints[num][1];

    //Tell the client that this is our id for the server
    socket.emit('register', {id: thisPlayerID});
    //Tell myself that I have spawned
    socket.emit('spawn', player);
    //Tell others that I have spawned
    socket.broadcast.emit('spawn', player);

    //Tell myself about everyone else in the game
    for(var playerID in players) {
        if(playerID != thisPlayerID)
        {
            socket.emit('spawn', players[playerID]);
        }
    }

    //Update the Player's Position based on the Inputs received from the client
    socket.on('move', function(data) {
        var clientInputs = new Vector2(data.clientInputs.x, data.clientInputs.y);
        var speed = data.speed;
        var deltaTime = data.deltaTime;
        var timeSent = data.timeSent;

        //The amount of change the position will undergo
        var newDeltaPositions = new Vector2(clientInputs.x * speed * deltaTime, 
            clientInputs.y * speed * deltaTime);

        player.position.x += newDeltaPositions.x;
        player.position.y += newDeltaPositions.y;

        //Round to two decimal places
        player.position.x = Math.round(player.position.x * 100) / 100;
        player.position.y = Math.round(player.position.y * 100) / 100;

        //Creates a Move Message to be sent to the clients
        var move = new Move(thisPlayerID, timeSent, player.position);

        socket.broadcast.emit('move', move);
        socket.emit('move', move);
    });

    socket.on('collisionMove', function(data) {
        var clientInputs = new Vector2(data.clientInputs.x, data.clientInputs.y);
        var clientPosition = new Vector2(data.clientPosition.x, data.clientPosition.y);
        var speed = data.speed;
        var deltaTime = data.deltaTime;

        var deltaXPosition = clientInputs.x * speed * deltaTime;
        var deltaYPosition = clientInputs.y * speed * deltaTime;

        var xRange = new Vector2(player.position.x - deltaXPosition, player.position.x + deltaXPosition);
        var yRange = new Vector2(player.position.y - deltaYPosition, player.position.y + deltaYPosition);

        player.position.x = clientPosition.x;
        player.position.y = clientPosition.y;

        var collisionMove = new CollisionMove(thisPlayerID)
        collisionMove.position = player.position;

        socket.broadcast.emit('collisionMove', collisionMove);
    });

    socket.on('aim', function(data) {
        player.rotation = data.rotation;

        var aim = new Aim(thisPlayerID, data.rotation);

        socket.broadcast.emit('aim', aim);
    });

    socket.on('seatMove', function(data) {
        var x = data.position.x;
        var y = data.position.y;
        var rotation = data.rotation;

        player.position.x = x;
        player.position.y = y;
        player.rotation = rotation;

        var seatMove = new SeatMove();
        seatMove.id = thisPlayerID;
        seatMove.position.x = x;
        seatMove.position.y = y;
        seatMove.rotation = rotation;

        socket.broadcast.emit('seatMove', seatMove);
        socket.emit('seatMove', seatMove);
    });

    socket.on('attack', function() {
        socket.broadcast.emit('attack', {id: thisPlayerID});
    });

    socket.on('interact', function() {
        socket.broadcast.emit('interact', {id: thisPlayerID});
    });

    socket.on('takeDamage', function(data) {
        var senderID = data.senderID;
        var receiverID = data.receiverID;
        var damage = data.damage;

        console.log('myID: ' + thisPlayerID);
        console.log('senderID: ' + senderID);

        if(thisPlayerID === senderID)
        {
            console.log('Take Damage');

            var takeDamage = new TakeDamage(receiverID, damage);

            socket.emit('takeDamage', takeDamage);
            socket.broadcast.emit('takeDamage', takeDamage);
        }
    });

    socket.on('respawn', function() {
        var num = Math.floor((Math.random() * 6));
        player.position.x = spawnPoints[num][0];
        player.position.y = spawnPoints[num][1];

        socket.emit('respawn', player);
        socket.broadcast.emit('respawn', player);
    });

    socket.on('disconnect', function() {
        console.log('A player has disconnected');
        delete players[thisPlayerID];
        delete sockets[thisPlayerID];
        socket.broadcast.emit('disconnected', player);
    });
});
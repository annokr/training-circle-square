import '@riotjs/hot-reload'
import { mount, register, install } from 'riot'
import Hub from './hub'
import GameBoard from './components/game-board/game-board.riot'
import OxField from './components/ox-field/ox-field.riot'
import ResetButton from './components/reset-button/reset-button.riot'
import ExampleText from './components/example-text/example-text.riot'
import CurrentGame from './components/current-game/current-game.riot'

var appState = {
    currentPlayer: 'X',
    oxFields: {}
};

// SignalR
var hub = new Hub("http://localhost:5000/xohub");

// ----------------------------------
// SignalR calls from backend go here

hub.connection.on("CurrentFieldValue", (fieldId, value) => {
    appState.oxFields[fieldId].update({value})
})

// SignalR calls from backend go here
// ----------------------------------

// RiotJs
install(function(component) {
    component.appState = appState;
    component.hub = hub.connection;
})

// -----------------------------------------------
// RiotJs component registration happens here here

register('ox-field', OxField)
register('game-board', GameBoard)
register('reset-button', ResetButton)
register('example-text', ExampleText)
register('current-game', CurrentGame)

// RiotJs component registration happens here here
// -----------------------------------------------

async function StartApp() {
    await hub.start()
    mount('game-board')
}

StartApp()

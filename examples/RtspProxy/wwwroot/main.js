'use strict'

const isSecure = location.protocol === 'https:'
const wsProtocol = isSecure ? 'wss:' : 'ws:'
const serverConnection = new WebSocket(`${wsProtocol}//${document.location.host}/ws`)
serverConnection.onmessage = wsRecvMessage
serverConnection.onopen = () => {
    createPeerConnection()
    wsSendMessage({
        action: WSAction.CreateSdpOffer
    })
}

let peerConnection

const WSAction = {
    CreateSdpOffer: 0,
    SelectIceCandidate: 1,
    PlayStream: 2
}

function wsSendMessage(msg) {
    const msgJson = JSON.stringify(msg)
    log(`WS Send: ${msgJson}`)
    return serverConnection.send(msgJson)
}

async function wsRecvMessage(msg) {
    log('WS Recv: ' + msg.data)

    const msgData = JSON.parse(msg.data)
    await processWebSocketMessage(msgData)
}

async function playStream(e) {
    e.preventDefault()

    try {
        const rtspStreamUrlInput = document.getElementById('rtspStreamUrlInput')

        await wsSendMessage({
            action: WSAction.PlayStream,
            rtspUrl: rtspStreamUrlInput.value
        })
        await wsSendMessage({
            action: WSAction.CreateSdpOffer
        })
    } catch (e) {
        throw new Error(e.message)
    }

    return false
}

function log(message) {
    const logMessage = `[${new Date().toLocaleTimeString()}] ${message}`
    console.log(logMessage)
    const logDiv = document.getElementById('logDiv')
    const logEntryDiv = document.createElement('div')
    logEntryDiv.innerText = logMessage
    logDiv.appendChild(logEntryDiv)
    logDiv.scrollTo(0, logDiv.scrollHeight)
}

function setVideoEvents(videoElementId) {
    const videoElement = document.getElementById(videoElementId)

    videoElement.addEventListener('loadedmetadata', function () {
        log(`Remote video size: ${this.videoWidth}x${this.videoHeight} px`)
    })

    videoElement.addEventListener('resize', () => {
        log(`Remote video size changed to ${videoElement.videoWidth}x${videoElement.videoHeight}px`)
    })

    videoElement.addEventListener("playing", function () {
        log('Playing')
        const localSdpDiv = document.getElementById('localSdpDiv')
        localSdpDiv.innerText = peerConnection.localDescription.sdp
        const remoteSdpDiv = document.getElementById('remoteSdpDiv')
        remoteSdpDiv.innerText = peerConnection.remoteDescription.sdp
    })
}
setVideoEvents('remoteVideo1')

async function processWebSocketMessage(messageData) {
    const signal = messageData
    if (signal.sdp) {
        await peerConnection.setRemoteDescription(signal)

        const mediaConstraints = {
            mandatory: {
                offerToReceiveAudio: true,
                offerToReceiveVideo: true
            }
        }

        const ans = await peerConnection.createAnswer(mediaConstraints)

        await peerConnection.setLocalDescription(ans)
    }
}

function createPeerConnection() {
    log('Creating PeerConnection')
    peerConnection = new RTCPeerConnection()

    peerConnection.addEventListener('icecandidate', e => {
        const iceCandidate = e.candidate || {}

        if (iceCandidate.candidate) log(`ICE candidate: ${iceCandidate.candidate}`)
    })
    peerConnection.addEventListener('track', e => {
        const remoteVideo = document.getElementById('remoteVideo1')

        log(`Received Remote Streams. Count: ${e.streams.length}`)

        const stream = e.streams[0]
        if (remoteVideo.srcObject !== stream) {
            remoteVideo.srcObject = stream
            const tracks = stream.getVideoTracks()
            log(`Received Remote Tracks. Count: ${tracks.length}`)
        }
    })
    peerConnection.oniceconnectionstatechange = e => {
        log(`ICE Connection SignalingState: ${e.target.signalingState}`)
        if (e.target.signalingState === 'stable') {
            wsSendMessage({
                action: WSAction.SelectIceCandidate,
                sdp: e.target.localDescription.sdp
            })
        }
    }
}

window.setInterval(async () => {
    const stats = await peerConnection.getStats()

    for (const stat of stats) {
        const statDetail = stat[1]
        if (statDetail.type === 'transport') {
            document.getElementById('bytesReceivedSpan').innerText = statDetail.bytesReceived
            document.getElementById('bytesSentSpan').innerText = statDetail.bytesSent
            document.getElementById('dtlsCipherSpan').innerText = statDetail.dtlsCipher
            document.getElementById('dtlsStateSpan').innerText = statDetail.dtlsState
            document.getElementById('srtpCipherSpan').innerText = statDetail.srtpCipher
            document.getElementById('timestampSpan').innerText = new Date(statDetail.timestamp).toLocaleTimeString()
        }
    }
}, 100)
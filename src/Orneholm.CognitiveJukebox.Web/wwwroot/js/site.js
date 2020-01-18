(function (window) {
    'use strict';

    var config = window.jukeboxai.config;
    var cbRoot = document.querySelector('#cb-root');

    var audio = null;
    var fadeAudio;
    var quoteIndex = 0;

    init();

    function init() {
        var webcamWrapperElement = cbRoot.querySelector('.cb-webcam-wrapper');
        var videoElement = cbRoot.querySelector('.cb-webcam-video');

        webcamWrapperElement.addEventListener('click', function (e) {
            stopSound();
            if (cbRoot.classList.contains('cb-activated')) {
                cbRoot.classList.remove('cb-activated');
                setTimeout(function () {
                    initAnalyze();
                }, 1200);
            } else {
                initAnalyze();
            }
        });

        initCamera(videoElement);
    }

    function initCamera(videoElement) {
        return new Promise(function (resolve, reject) {
            navigator.mediaDevices.getUserMedia({
                audio: false,
                video: true
            }).then(function (mediaStream) {
                videoElement.srcObject = mediaStream;
                videoElement.addEventListener('loadedmetadata', function (e) {
                    videoElement.play();
                    window.setTimeout(function () {
                        resolve();
                    }, 500);
                });
            });
        });
    }

    function initAnalyze() {
        var videoElement = cbRoot.querySelector('.cb-webcam-video');
        showTakePhoto();
        analyzeVideoFrame(videoElement).then(function (result) {
            console.log('Webcam image analyzed', result);

            showAnalyzeResult({
                description: result.imageDescription,
                faces: result.faces,
                musicYear: result.musicYear
            }, result.musicTracks);
        });
    }

    function showTakePhoto() {
        var webcamWrapperElement = cbRoot.querySelector('.cb-webcam-wrapper');
        var videoElement = cbRoot.querySelector('.cb-webcam-video');

        if (config.shutterSoundUrl) {
            playSound(config.shutterSoundUrl);
        }

        cbRoot.classList.remove('cb-info');
        webcamWrapperElement.classList.add('cb-webcam-flash');
        videoElement.classList.add('cb-webcam-flash');

        setTimeout(function () {
            webcamWrapperElement.classList.remove('cb-webcam-flash');
            videoElement.classList.remove('cb-webcam-flash');
        }, 700);
    }

    function analyzeVideoFrame(video) {
        var blob = videoToBlob(video);
        return getImageAnalyze(blob);
    }

    function getImageAnalyze(blob) {
        var formData = new FormData();
        formData.append('file', blob);

        return fetch(config.apiBaseUrl + 'image/analyze', {
            method: 'POST',
            body: formData
        }).then(function (data) {
            return data.json();
        });
    }

    function showMusic(face, musicYear, musicTracks) {
        var musicTrack = musicTracks[0];
        var songImageElement = cbRoot.querySelector('.cb-song-image');
        var songDescriptionElement = cbRoot.querySelector('.cb-song-description');
        var songProgressElement = cbRoot.querySelector('.cb-song-progress');

        cbRoot.classList.add('cb-activated');

        songDescriptionElement.innerText = musicTrack.trackName + ' by ' + musicTrack.artistName;
        songImageElement.src = musicTrack.albumCoverUrl;
        songProgressElement.style.width = 0;

        function onProgress(percentage) {
            songProgressElement.style.width = Math.min(100, percentage * 100) + '%';
        }

        stopSound();

        var personDescription = 'According to my AI, you look like ' + face.age + ' years old.';
        var musicDescription = 'I will play ' + musicTrack.trackName + ' by ' + musicTrack.artistName + '. A great album from ' + musicYear;
        var suffix = getNextQuote();

        speak([personDescription, musicDescription, suffix].join(' ')).then(function () {
            playSound(musicTrack.trackAudioPreviewUrl, true, onProgress).then(function () {
                cbRoot.classList.remove('cb-activated');
                setTimeout(function () {
                    initAnalyze();
                }, 1250);
            });
        });
    }

    function showAnalyzeResult(image, musicTracks) {
        var descriptionElement = cbRoot.querySelector('.cb-webcam-description');

        if (image.faces.length > 0) {
            descriptionElement.innerText = image.description;
            var face = image.faces[0];
            showMusic(face, image.musicYear, musicTracks);
        } else {
            descriptionElement.innerText = 'Could not find any face in the picture.';
            cbRoot.classList.add('cb-info');
        }
    }

    function getNextQuote() {
        if (!config.quotes.length) {
            return '';
        }

        var quote = config.quotes[quoteIndex % config.quotes.length];
        quoteIndex++;
        return quote;
    }

    /* Helpers */

    function speak(text) {
        return new Promise(function (resolve, reject) {
            if (!window.speechSynthesis || !SpeechSynthesisUtterance) {
                console.log(text);
                resolve();
                return;
            }

            var message = new SpeechSynthesisUtterance(text);
            message.onend = function (e) {
                resolve();
            };
            window.speechSynthesis.speak(message);
        });
    }

    function stopSound() {
        audio = audio || document.createElement('audio');
        cbRoot.appendChild(audio);
        audio.pause();
        audio.currentTime = 0;
        audio.volume = 1;
    }

    function playSound(url, fade, onProgress) {
        fade = fade || false;
        clearInterval(fadeAudio);
        return new Promise(function (resolve, reject) {
            stopSound();
            audio.src = url;
            audio.addEventListener('loadedmetadata', function (e) {
                audio.play();
            });
            if (typeof (onProgress) === 'function') {
                audio.addEventListener('timeupdate', function (e) {
                    onProgress(audio.currentTime / audio.duration);
                });
            }

            audio.addEventListener('ended', function () {
                resolve();
            });

            if (fade) {
                audio.volume = 0;
                var fadePoint = audio.duration - 2;
                fadeAudio = setInterval(function () {
                    if (audio.currentTime <= 2) {
                        setAudioVolume(audio, 0.1);
                    }
                    if (audio.currentTime >= fadePoint) {
                        setAudioVolume(audio, -0.1);
                    }

                    if (audio.volume <= 0.0) {
                        clearInterval(fadeAudio);
                    }
                }, 200);
            }
        });
    }

    function setAudioVolume(audio, volumeDiff) {
        var newVolume = audio.volume + volumeDiff;
        audio.volume = Math.min(1.0, Math.max(0.0, newVolume));
    }

    function videoToImageDataURI(video) {
        var canvas = document.createElement("canvas");
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        canvas.getContext('2d')
            .drawImage(video, 0, 0, canvas.width, canvas.height);

        return canvas.toDataURL();
    }

    function dataURItoBlob(dataURI, type) {
        var byteString = atob(dataURI.split(',')[1]);
        var ab = new ArrayBuffer(byteString.length);
        var ia = new Uint8Array(ab);
        for (var i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }
        return new Blob([ab], {
            type: type
        });
    }

    function videoToBlob(video) {
        var dataURI = videoToImageDataURI(video);
        var blob = dataURItoBlob(dataURI, 'image/png');
        return blob;
    }
}(window));

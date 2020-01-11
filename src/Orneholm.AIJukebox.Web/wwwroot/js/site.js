(function () {
    'use strict';

    var config = jukeboxai.config;
    var keys = jukeboxai.keys;

    var audio = null;
    var fadeAudio;
    var quoteIndex = 0;

    function getNextQuote() {
        var quote = config.quotes[quoteIndex % config.quotes.length];
        quoteIndex++;
        return quote;
    }

    function stopSound() {
        audio = audio || document.createElement('audio');
        document.body.appendChild(audio);
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
                    if ((audio.currentTime <= 2) && (audio.volume < 1.0)) {
                        audio.volume += 0.1;
                    }
                    if ((audio.currentTime >= fadePoint) && (audio.volume !== 0.0)) {
                        audio.volume -= 0.1;
                    }
                    if (audio.volume === 0.0) {
                        clearInterval(fadeAudio);
                    }
                }, 200);
            }
        });
    }

    function getRandomFromArray(items) {
        if (!items || items.length === 0) {
            return null;
        }

        return items[Math.floor(Math.random() * items.length)];
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

    function analyzeVideoFrame(video) {
        var blob = videoToBlob(video);
        return getImageAnalyze(blob);
    }

    function getTrack(query) {
        return fetch('https://api.spotify.com/v1/search?type=track&q=' + query, {
            headers: {
                'Authorization': 'Bearer ' + keys.spotifyBearerToken
            }
        })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.tracks.length === 0) {
                    return null;
                }
                var nonExplicitItems = data.tracks.items.filter(x => !x.explicit);
                return getRandomFromArray(nonExplicitItems);
            });
    }

    function getRelevantTag(tags) {
        if (!tags || tags.length === 0) {
            return null;
        }

        var relevantTags = tags.filter(function (t) {
            return config.irrelevantTags.indexOf(t) === -1;
        });

        if (relevantTags.length > 3) {
            relevantTags = relevantTags.slice(1, 3);
        }

        return getRandomFromArray(relevantTags) || getRandomFromArray(tags);
    }

    function showMusic(tags, relevantTag) {
        var element = document.querySelector('#analyze-music');
        if (!relevantTag) {
            element.innerText = '';
            return;
        }
        var query = relevantTag;

        getTrack(query).then(function (track) {
            if (!track) {
                element.innerText = '';
            }

            document.body.classList.add('activated');

            var artist = track.artists[0].name;

            var progress = document.createElement('div');
            progress.classList.add('song-progress');

            var label = document.createElement('div');
            label.innerText = track.name + ' by ' + artist;
            label.appendChild(progress);

            var image = document.createElement('img');
            image.src = track.album.images[0].url;
            image.classList.add('embed-responsive-item');

            element.innerText = '';
            element.appendChild(image);
            element.appendChild(label);

            var suffix = getNextQuote();

            function onProgress(percentage) {
                progress.style.width = Math.min(100, percentage * 100) + '%';
            }

            stopSound();
            speak('I found ' + relevantTag + ' in the picture. I will play ' + track.name + ' by ' + artist + '. ' + suffix).then(function () {
                playSound(track.preview_url, true, onProgress).then(function () {
                    document.body.classList.remove('activated');
                    setTimeout(function () {
                        initAnalyze();
                    }, 1250);
                });
            });
        });
    }

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

    function showAnalyzeResult(caption, tags, relevantTag) {
        var element = document.querySelector('#analyze-tags');
        var list = document.createElement('ul');
        list.classList.add('list-unstyled');
        list.classList.add('clearfix');
        tags.forEach(function (t) {
            var tagElement = document.createElement('li');
            tagElement.innerText = t;
            tagElement.classList.add('badge');
            if (relevantTag === t) {
                tagElement.classList.add('badge-primary');
            } else {
                tagElement.classList.add('badge-default');
            }
            list.appendChild(tagElement);
        });
        element.innerText = '';
        element.appendChild(list);

        showMusic(tags, relevantTag);
    }

    function showTakePhoto() {
        var video = document.querySelector('#videopreview');
        var camera = document.querySelector('#camera');

        if (config.shutterSoundUrl) {
            playSound(config.shutterSoundUrl);
        }
        video.classList.add('flash');
        camera.classList.add('flash');

        setTimeout(function () {
            video.classList.remove('flash');
            camera.classList.remove('flash');
        }, 700);
    }

    function initAnalyze() {
        var video = document.querySelector('#videopreview');
        showTakePhoto();
        analyzeVideoFrame(video).then(function (result) {
            console.log('Image analyze', result);
            var caption = result.description;
            var tags = result.relevantTags;
            var relevantTag = getRelevantTag(tags);
            showAnalyzeResult(caption, tags, relevantTag);
        });
    }

    function initCamera(video) {
        return new Promise(function (resolve, reject) {
            navigator.mediaDevices.getUserMedia({
                audio: false,
                video: true
            }).then(function (mediaStream) {
                video.srcObject = mediaStream;
                video.addEventListener('loadedmetadata', function (e) {
                    video.play();
                    window.setTimeout(function () {
                        resolve();
                    }, 500);
                });
            });
        });
    }

    function init() {
        var video = document.querySelector('#videopreview');
        var camera = document.querySelector('#camera');
        camera.addEventListener('click', function (e) {
            stopSound();
            if (document.body.classList.contains('activated')) {
                document.body.classList.remove('activated');
                setTimeout(function () {
                    initAnalyze();
                }, 1200);
            } else {
                initAnalyze();
            }
        });
        initCamera(video);
    }

    init();
}());

document.addEventListener('DOMContentLoaded', function() {
    const videoPlayer = document.getElementById('videoPlayer');
    if (!videoPlayer) return;

    const videoUrl = videoPlayer.dataset.url;
    if (!videoUrl) return;

    // Определяем тип видео и создаем соответствующий плеер
    if (isYouTubeUrl(videoUrl)) {
        createYouTubePlayer(videoPlayer, videoUrl);
    } else if (isVimeoUrl(videoUrl)) {
        createVimeoPlayer(videoPlayer, videoUrl);
    } else {
        createGenericPlayer(videoPlayer, videoUrl);
    }
});

function isYouTubeUrl(url) {
    return /(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)/.test(url);
}

function isVimeoUrl(url) {
    return /vimeo\.com/.test(url);
}

function createYouTubePlayer(container, url) {
    const videoId = extractYouTubeId(url);
    if (!videoId) {
        createGenericPlayer(container, url);
        return;
    }

    const iframe = document.createElement('iframe');
    iframe.src = `https://www.youtube.com/embed/${videoId}`;
    iframe.width = '100%';
    iframe.height = '450';
    iframe.frameBorder = '0';
    iframe.allowFullscreen = true;
    iframe.className = 'video-player';
    
    container.appendChild(iframe);
}

function createVimeoPlayer(container, url) {
    const videoId = extractVimeoId(url);
    if (!videoId) {
        createGenericPlayer(container, url);
        return;
    }

    const iframe = document.createElement('iframe');
    iframe.src = `https://player.vimeo.com/video/${videoId}`;
    iframe.width = '100%';
    iframe.height = '450';
    iframe.frameBorder = '0';
    iframe.allowFullscreen = true;
    iframe.className = 'video-player';
    
    container.appendChild(iframe);
}

function createGenericPlayer(container, url) {
    // Для других типов видео создаем простую ссылку или video элемент
    if (isDirectVideoUrl(url)) {
        const video = document.createElement('video');
        video.src = url;
        video.controls = true;
        video.className = 'video-player';
        video.style.width = '100%';
        video.style.maxHeight = '450px';
        
        container.appendChild(video);
    } else {
        // Создаем ссылку для неподдерживаемых форматов
        const linkContainer = document.createElement('div');
        linkContainer.className = 'video-link-container';
        linkContainer.innerHTML = `
            <div class="video-placeholder">
                <i class="bi bi-play-circle" style="font-size: 4rem; color: #6c757d;"></i>
                <h4>Внешнее видео</h4>
                <p>Нажмите на ссылку ниже, чтобы открыть видео:</p>
                <a href="${url}" target="_blank" class="btn btn-primary">
                    <i class="bi bi-box-arrow-up-right"></i> Открыть видео
                </a>
            </div>
        `;
        
        container.appendChild(linkContainer);
    }
}

function extractYouTubeId(url) {
    const match = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&\n?#]+)/);
    return match ? match[1] : null;
}

function extractVimeoId(url) {
    const match = url.match(/vimeo\.com\/(\d+)/);
    return match ? match[1] : null;
}

function isDirectVideoUrl(url) {
    return /\.(mp4|webm|ogg|avi|mov)$/i.test(url);
}

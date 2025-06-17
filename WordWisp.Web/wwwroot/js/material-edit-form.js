document.addEventListener('DOMContentLoaded', function() {
    const fileInput = document.getElementById('fileInput');
    const fileUploadArea = document.getElementById('fileUploadArea');
    const filePreview = document.getElementById('filePreview');

    // Проверяем, есть ли текущий файл
    const hasCurrentFile = document.querySelector('.current-file-section') !== null;
    if (hasCurrentFile && fileUploadArea) {
        fileUploadArea.classList.add('has-current-file');
    }

    // Обработка загрузки файлов (аналогично create форме)
    if (fileInput && fileUploadArea) {
        // Drag & Drop
        fileUploadArea.addEventListener('dragover', function(e) {
            e.preventDefault();
            this.classList.add('dragover');
        });

        fileUploadArea.addEventListener('dragleave', function(e) {
            e.preventDefault();
            this.classList.remove('dragover');
        });

        fileUploadArea.addEventListener('drop', function(e) {
            e.preventDefault();
            this.classList.remove('dragover');
            
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                fileInput.files = files;
                showFilePreview(files[0]);
            }
        });

        // Выбор файла
        fileInput.addEventListener('change', function() {
            if (this.files.length > 0) {
                showFilePreview(this.files[0]);
            }
        });
    }

    function showFilePreview(file) {
        if (!filePreview) return;

        const fileName = filePreview.querySelector('.file-name');
        const fileSize = filePreview.querySelector('.file-size');
        const placeholder = document.querySelector('.file-upload-placeholder');

        if (fileName) fileName.textContent = file.name;
        if (fileSize) fileSize.textContent = formatFileSize(file.size);

        if (placeholder) placeholder.style.display = 'none';
        filePreview.style.display = 'flex';
    }

    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Глобальная функция для удаления файла
    window.removeFile = function() {
        if (fileInput) fileInput.value = '';
        if (filePreview) filePreview.style.display = 'none';
        
        const placeholder = document.querySelector('.file-upload-placeholder');
        if (placeholder) placeholder.style.display = 'block';
    };

    // Валидация формы
    const form = document.getElementById('editMaterialForm');
    if (form) {
        form.addEventListener('submit', function(e) {
            // Дополнительная валидация при необходимости
            const title = document.querySelector('input[name="Input.Title"]');
            if (!title || !title.value.trim()) {
                e.preventDefault();
                alert('Пожалуйста, введите название материала');
                return;
            }
        });
    }
});

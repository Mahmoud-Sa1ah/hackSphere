import { Component, ElementRef, EventEmitter, forwardRef, Input, Output, ViewChild, HostListener, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideBold, lucideItalic, lucideList, lucideListOrdered, lucideImage, lucideHeading1, lucideHeading2, lucideQuote, lucideCode, lucideAlignLeft, lucideAlignCenter, lucideAlignRight } from '@ng-icons/lucide';

@Component({
    selector: 'app-rich-text-editor',
    standalone: true,
    imports: [CommonModule, NgIconComponent],
    templateUrl: './rich-text-editor.component.html',
    styleUrl: './rich-text-editor.component.css',
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => RichTextEditorComponent),
            multi: true
        },
        provideIcons({ lucideBold, lucideItalic, lucideList, lucideListOrdered, lucideImage, lucideHeading1, lucideHeading2, lucideQuote, lucideCode, lucideAlignLeft, lucideAlignCenter, lucideAlignRight })
    ]
})
export class RichTextEditorComponent implements ControlValueAccessor, AfterViewInit, OnDestroy {
    @Input() placeholder = 'Type something...';
    @Output() imageUpload = new EventEmitter<{ file: File, callback: (url: string) => void }>();

    @ViewChild('editor') editorMsg!: ElementRef<HTMLDivElement>;

    content = '';
    isDisabled = false;
    uploading = false;

    // Resizing state
    selectedImage: HTMLImageElement | null = null;
    resizing = false;
    resizeStartX = 0;
    resizeStartWidth = 0;

    // Overlay position
    overlayTop = 0;
    overlayLeft = 0;
    overlayWidth = 0;
    overlayHeight = 0;

    onChange: (value: string) => void = () => { };
    onTouched: () => void = () => { };

    writeValue(value: string): void {
        this.content = value || '';
        if (this.editorMsg && this.editorMsg.nativeElement) {
            if (this.editorMsg.nativeElement.innerHTML !== this.content) {
                this.editorMsg.nativeElement.innerHTML = this.content;
            }
        }
    }

    ngAfterViewInit() {
        if (this.content && this.editorMsg && this.editorMsg.nativeElement) {
            if (this.editorMsg.nativeElement.innerHTML !== this.content) {
                this.editorMsg.nativeElement.innerHTML = this.content;
            }
        }
    }

    ngOnDestroy() {
        document.removeEventListener('mousemove', this.doResize);
        document.removeEventListener('mouseup', this.stopResize);
    }

    registerOnChange(fn: (value: string) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    onInput(event: Event) {
        const html = (event.target as HTMLElement).innerHTML;
        this.content = html;
        this.onChange(html);
    }

    onBlur() {
        this.onTouched();
        // Ensure final content is saved
        if (this.editorMsg && this.editorMsg.nativeElement) {
            this.onInput({ target: this.editorMsg.nativeElement } as any);
        }
    }

    execCommand(command: string, value: string | undefined = undefined) {
        document.execCommand(command, false, value);
        this.editorMsg.nativeElement.focus();
        // Update model
        this.onInput({ target: this.editorMsg.nativeElement } as any);
    }

    triggerImageUpload() {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';
        input.onchange = (e: any) => {
            const file = e.target.files[0];
            if (file) {
                this.uploading = true;
                this.imageUpload.emit({
                    file,
                    callback: (url: string) => this.insertImage(url)
                });
            }
        };
        input.click();
    }

    insertImage(url: string) {
        const imgHtml = `<img src="${url}" class="editor-image" style="max-width: 100%; border-radius: 6px; cursor: pointer;">`;
        this.execCommand('insertHTML', imgHtml);
        this.uploading = false;
    }

    // Formatting helpers
    get isActiveBold() { return document.queryCommandState('bold'); }
    get isActiveItalic() { return document.queryCommandState('italic'); }

    // Alignment
    alignImage(alignment: 'left' | 'center' | 'right') {
        if (!this.selectedImage) return;

        this.selectedImage.style.display = 'block';
        this.selectedImage.style.marginLeft = alignment === 'center' || alignment === 'right' ? 'auto' : '0';
        this.selectedImage.style.marginRight = alignment === 'center' || alignment === 'left' ? 'auto' : '0';

        // Trigger update
        this.onInput({ target: this.editorMsg.nativeElement } as any);
        setTimeout(() => this.updateOverlayPosition(), 0);
    }

    // Drag and Drop
    onDrop(event: DragEvent) {
        event.preventDefault();
        if (event.dataTransfer && event.dataTransfer.files.length > 0) {
            const file = event.dataTransfer.files[0];
            if (file.type.startsWith('image/')) {
                this.imageUpload.emit({
                    file,
                    callback: (url: string) => this.insertImage(url)
                });
            }
        }
    }

    onDragOver(event: DragEvent) {
        event.preventDefault();
    }

    // Image Resizing Logic
    @HostListener('document:click', ['$event'])
    onDocumentClick(event: MouseEvent) {
        const target = event.target as HTMLElement;
        // If click is OUTSIDE the editor, deselect
        if (this.editorMsg && this.editorMsg.nativeElement && !this.editorMsg.nativeElement.contains(target)) {
            this.selectedImage = null;
        }
    }

    handleEditorClick(event: MouseEvent) {
        const target = event.target as HTMLElement;
        if (target.tagName === 'IMG') {
            // Select image
            this.selectedImage = target as HTMLImageElement;
            this.updateOverlayPosition();
            // Prevent this click from bubbling to document and triggering deselect (though our logic handles checking contains)
            event.stopPropagation();
        } else if (!this.resizing && !target.closest('.resize-overlay')) {
            // Clicked inside editor but not on image or overlay -> deselect
            this.selectedImage = null;
        }
    }

    updateOverlayPosition() {
        if (!this.selectedImage || !this.editorMsg) return;

        const editorRect = this.editorMsg.nativeElement.getBoundingClientRect();
        const imgRect = this.selectedImage.getBoundingClientRect();

        this.overlayTop = imgRect.top - editorRect.top + this.editorMsg.nativeElement.scrollTop;
        this.overlayLeft = imgRect.left - editorRect.left + this.editorMsg.nativeElement.scrollLeft;
        this.overlayWidth = imgRect.width;
        this.overlayHeight = imgRect.height;
    }

    startResize(event: MouseEvent) {
        if (!this.selectedImage) return;
        event.preventDefault();
        event.stopPropagation();
        this.resizing = true;
        this.resizeStartX = event.clientX;
        this.resizeStartWidth = this.selectedImage.offsetWidth;

        document.addEventListener('mousemove', this.doResize);
        document.addEventListener('mouseup', this.stopResize);
    }

    doResize = (e: MouseEvent) => {
        if (!this.resizing || !this.selectedImage) return;
        const dx = e.clientX - this.resizeStartX;
        const newWidth = Math.max(50, this.resizeStartWidth + dx);
        this.selectedImage.style.width = `${newWidth}px`;
        this.selectedImage.style.maxWidth = '100%'; // Ensure it doesn't overflow
        this.updateOverlayPosition();
    };

    stopResize = () => {
        this.resizing = false;
        document.removeEventListener('mousemove', this.doResize);
        document.removeEventListener('mouseup', this.stopResize);
        // Trigger update to save changes
        if (this.editorMsg && this.editorMsg.nativeElement) {
            this.onInput({ target: this.editorMsg.nativeElement } as any);
        }
    };
}

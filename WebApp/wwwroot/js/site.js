$(document).ready(function () {
    function fetchMessages() {
        $.ajax({
            url: '/api/messages',
            method: 'GET',
            dataType: 'json',
            success: function (response) {
                if (response.messages) {
                    const messagesArray = Object.keys(response.messages).map(function(messageId) {
                        const message = response.messages[messageId];
                        return {
                            id: messageId,
                            peer_id: message.peer_id,
                            message: message.message,
                            time: new Date(parseInt(messageId, 10))
                        };
                    });

                    messagesArray.sort(function(a, b) {
                        return b.time - a.time;
                    });

                    $('#messagesList').empty();

                    messagesArray.forEach(function(message) {
                        const sanitizedMessage = DOMPurify.sanitize(message.message.replace(/</g, '&lt;').replace(/>/g, '&gt;'));
                        
                        $('#messagesList').append(
                            $('<li class="list-group-item"></li>').html(`
                                <strong>${message.peer_id}:</strong> 
                                <span class="message-text">${sanitizedMessage}</span>
                                <span class="badge rounded-pill bg-secondary">${message.time.toLocaleString()}</span>
                            `)
                        );

                        $('.message-text').text(function(index, text) {
                            return text;
                        });
                    });
                } else {
                    console.error('Invalid response format: messages key not found.');
                }
            },
            error: function (error) {
                console.error('Error fetching messages:', error);
            }
        });
    }
    $('#sendMessageForm').on('submit', function (event) {
        event.preventDefault();

        const message = $('#messageInput').val();

        if (message) {
            $.ajax({
                url: `/api/send?message=${encodeURIComponent(message)}`,
                type: 'GET',
                success: function () {
                    $('#messageInput').val('');
                    fetchMessages();
                },
                error: function (error) {
                    console.error('Error sending message:', error);
                }
            });
        }
    });

    fetchMessages();
    setInterval(fetchMessages, 5000);
});
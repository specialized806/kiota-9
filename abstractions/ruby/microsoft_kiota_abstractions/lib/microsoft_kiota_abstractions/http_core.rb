require_relative 'request_info'
require_relative 'response_handler'

module MicrosoftKiotaAbstractions
  module HttpCore
    include ResponseHandler

    def sendAsync(request_info=RequestInfo.new, type, response_handler=ResponseHandler)
      raise NotImplementedError.new
    end

  end
end
